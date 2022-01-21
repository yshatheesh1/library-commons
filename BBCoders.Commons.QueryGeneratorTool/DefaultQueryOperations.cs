using System;
using System.Text.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BBCoders.Commons.QueryGeneratorTool.Models;
using BBCoders.Commons.QueryGeneratorTool.Services;
using BBCoders.Commons.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BBCoders.Commons.QueryGenerator;
using BBCoders.Commons.QueryGeneratorTool.Helpers;
using Microsoft.EntityFrameworkCore.Design;
using BBCoders.Commons.QueryGeneratorTool.Services.SqlGenerator;
using System.Text.RegularExpressions;

namespace BBCoders.Commons.QueryGeneratorTool
{
    public class DefaultQueryOperations : QueryGenerator.QueryContext
    {
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;
        private ISqlGenerationHelper _sqlGenerationHelper;
        private IRelationalTypeMappingSource _relationalTypeMappingSource;
        private MigrationsScaffolderDependencies _migrationsScaffolderDependencies;
        private DbContext _dbContext;
        private Language _language;
        private List<QueryModel> _sqlModel;
        private List<ITable> _tables;
        private QueryOptions _queryOptions;
        public DefaultQueryOperations([NotNull] Assembly assembly, [NotNull] Assembly startupAssembly, [NotNull] string projectDir, [NotNull] string rootNamespace, [NotNull] string[] designArgs = null) :
         base(assembly, startupAssembly, projectDir, rootNamespace, designArgs)
        {
            var handler = new OperationReportHandler(
                    (Action<string>)Reporter.WriteError,
                    (Action<string>)Reporter.WriteWarning,
                    (Action<string>)Reporter.WriteInformation,
                    (Action<string>)Reporter.WriteVerbose);
            var operationReporter = new OperationReporter(handler);
            Reporter.WriteVerbose("Initializing dependencies...");
            _contextOperations = new DbContextOperations(operationReporter, assembly, startupAssembly, projectDir, rootNamespace, "C#", false, designArgs);
            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, operationReporter, designArgs);
        }

        /// <summary>
        /// execute all query configurations using reflection
        /// </summary>
        public void Execute()
        {
            Reporter.WriteVerbose("Started generating source for queries...");
            var queryGenerators = GetQueryConfigurationTypes();
            Reporter.WriteVerbose($"Found {queryGenerators.Count()} query generators.");
            foreach (var queryGenerator in queryGenerators)
            {
                var contextType = GetDbContextType(queryGenerator);
                _dbContext = _contextOperations.CreateContext(contextType.Name);
                var services = _servicesBuilder.Build(_dbContext);
                using (var scope = services.CreateScope())
                {
                    _relationalTypeMappingSource = scope.ServiceProvider.GetService<IRelationalTypeMappingSource>();
                    _sqlGenerationHelper = scope.ServiceProvider.GetService<ISqlGenerationHelper>();
                    _migrationsScaffolderDependencies = scope.ServiceProvider.GetService<MigrationsScaffolderDependencies>();
                    // create instance of query configuration
                    var queryGeneratorInstance = Activator.CreateInstance(queryGenerator, new object[] { });
                    var queryConfigMethod = queryGenerator.GetMethod("GetQueryOptions");
                    _queryOptions = (QueryOptions)queryConfigMethod.Invoke(queryGeneratorInstance, new object[] { });
                    _tables = new List<ITable>();
                    _sqlModel = new List<QueryModel>();
                    // validate query options
                    if (_queryOptions == null
                        || string.IsNullOrWhiteSpace(_queryOptions.ModelFileName)
                        || string.IsNullOrWhiteSpace(_queryOptions.ModelOutputDirectory)
                        || string.IsNullOrWhiteSpace(_queryOptions.ModelPackageName)
                        || string.IsNullOrWhiteSpace(_queryOptions.ClassName)
                        || string.IsNullOrWhiteSpace(_queryOptions.PackageName)
                        || string.IsNullOrWhiteSpace(_queryOptions.OutputDirectory)
                        || _queryOptions.Language == null)
                    {
                        throw new ArgumentNullException(nameof(_queryOptions));
                    }
                    var (language, codeGenerator) = GetLanguageGenerator();
                    _language = language;
                    var createQuery = queryGenerator.GetMethod("CreateQuery");
                    createQuery.Invoke(queryGeneratorInstance, new object[] { _dbContext, this });
                    codeGenerator.Generate();
                }
            }
        }


        /// <summary>
        ///  adds default methods for a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Add<T>()
        {
            var table = _migrationsScaffolderDependencies.Model.GetRelationalModel()?.Tables
                .FirstOrDefault(x => x.EntityTypeMappings.First().EntityType.ClrType == typeof(T));
            if (table == null)
            {
                throw new Exception($"Table with given type not defined -{typeof(T)}. Please add this to DbContext if not added.");
            }
            _tables.Add(table);
        }


        /// <summary>
        ///  adds custom query by model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Add(QueryModel queryModel)
        {
            _sqlModel.Add(queryModel);
        }

        /// <summary>
        /// adds custom queries
        /// </summary>
        /// <param name="name"></param>
        /// <param name="inputParameters"></param>
        /// <param name="expression"></param>
        /// <typeparam name="T"></typeparam>
        public override void Add<T>(string name, List<ParameterExpression> inputParameters, Expression<T> expression)
        {
            Reporter.WriteVerbose("processing method - " + name);
            var customSqlModel = new QueryModel(name);
            var parameters = ParameterExpressionHelper.GetParameters(expression.Body.ToString(), inputParameters);
            var func = expression.Compile();
            var (bindingParameters, bindingDefaultValues) = parameters;
            var query = (IQueryable)func.GetType().GetMethod("Invoke").Invoke(func, bindingDefaultValues);
            if (query.Provider.Execute<IEnumerable>(query.Expression) is IRelationalQueryingEnumerable queryingEnumerable)
            {
                // add projections
                var projections = CreateProjections(queryingEnumerable);
                customSqlModel.Projections.AddRange(projections);
                // add sql
                var command = queryingEnumerable.CreateDbCommand();
                customSqlModel.Sql = command.CommandText.Replace("\n", "\n\t\t\t\t");
                // add where equal bindings
                var equalParameters = bindingParameters.Where(x => !x.InExpression).ToList();
                for (var i = 0; i < command.Parameters.Count; i++)
                {
                    var commandParameter = command.Parameters[i];
                    var userParameter = equalParameters.Count() > i ? equalParameters[i] : null;
                    // if user provides default value in the query, no user input is required
                    if (userParameter == null)
                    {
                        customSqlModel.Bindings.Add(new Binding()
                        {
                            Name = commandParameter.ParameterName,
                            DefaultValue = commandParameter.Value,
                            Value = null
                        });
                    }
                    else
                    {
                        var type = GetDbType(userParameter.Expression.Type);
                        var inputParameter = new Parameter()
                        {
                            Name = userParameter.Expression.Name,
                            IsList = false,
                            Type = type
                        };
                        customSqlModel.Parameters.Add(inputParameter);
                        customSqlModel.Bindings.Add(new Binding()
                        {
                            Name = commandParameter.ParameterName,
                            Value = inputParameter
                        });
                    }
                }
                // in bindings are special type, ef core doesn't create parameter because these are dynamic
                Regex rgx = new Regex(@"\s*IN\s*\(.*?\)");
                int index = 0;
                foreach (var binding in bindingParameters.Where(x => x.InExpression))
                {
                    var type = GetDbType(binding.Expression.Type.GetGenericArguments()[0]);
                    var inputParameter = new Parameter()
                    {
                        Name = binding.Expression.Name,
                        IsList = true,
                        Type = type
                    };

                    var match = rgx.Match(customSqlModel.Sql, index);
                    if (match.Success)
                    {
                        var placeholder = inputParameter.Name.GetJoinPlaceholder();
                        var inClause = $" IN (\" + {placeholder} + @\")";
                        customSqlModel.Sql = customSqlModel.Sql.Replace(match.Value, inClause);
                        index = match.Index + inClause.Length;
                    }
                    customSqlModel.Parameters.Add(inputParameter);
                    customSqlModel.Bindings.Add(new Binding()
                    {
                        Name = binding.Expression.Name,
                        Value = inputParameter
                    });
                }
            }
            string jsonString = JsonSerializer.Serialize(customSqlModel, new JsonSerializerOptions { WriteIndented = true });
            Reporter.WriteVerbose("Sql Model: " + jsonString);
            _sqlModel.Add(customSqlModel);
        }

        private String GetDbType(Type type)
        {
            var relationalTypeMapping = _relationalTypeMappingSource.GetMapping(type);
            type = relationalTypeMapping.DbType.HasValue ? SqlMapperHelper.getClrType(relationalTypeMapping.DbType.Value) :
                relationalTypeMapping.ClrType;
            return type.GetLanguageType(_language);
        }

        private Type GetRelationalType(RelationalTypeMapping relationalTypeMapping)
        {
            var type = relationalTypeMapping.DbType.HasValue ? SqlMapperHelper.getClrType(relationalTypeMapping.DbType.Value) :
                relationalTypeMapping.ClrType;
            return type;
        }

        private String GetDbType(RelationalTypeMapping relationalTypeMapping)
        {
            var type = GetRelationalType(relationalTypeMapping);
            return type.GetLanguageType(_language);
        }

        private List<Projection> CreateProjections(IRelationalQueryingEnumerable queryingEnumerable)
        {
            Reporter.WriteVerbose("creating projections...");
            var projections = new List<Projection>();
            var constantPropertyIndex = 0;
            var selectExpression = GetSelectExpression(queryingEnumerable);
            Reporter.WriteVerbose("Total projections found - " + selectExpression.Projection.Count());
            foreach (var projection in selectExpression.Projection)
            {
                var type = GetRelationalType(projection.Expression.TypeMapping);
                var sqlProjection = new Projection()
                {
                    Name = projection.Alias,
                    Type = type.Name,
                    IsNullable = false,
                    IsValueType = type.IsValueType
                };
                Reporter.WriteVerbose("projection : " + sqlProjection.Name);
                // if no name is provided for custom sql 
                if (string.IsNullOrEmpty(sqlProjection.Name))
                {
                    sqlProjection.Name = $"Value_{constantPropertyIndex}";
                    Reporter.WriteVerbose("projection has default value : " + sqlProjection.Name);
                    constantPropertyIndex++;
                }
                if (projection.Expression is ColumnExpression columnExpression)
                {
                    sqlProjection.IsNullable = columnExpression.IsNullable;
                    if (columnExpression.Table is TableExpression tableExpression)
                    {
                        sqlProjection.Name = projection.Alias;
                        sqlProjection.Table = tableExpression.Name;
                        Reporter.WriteVerbose("project table : " + sqlProjection.Table);
                    }
                    else if (columnExpression.Table is JoinExpressionBase joinExpressionBase && joinExpressionBase.Table is TableExpression tableExpression2)
                    {
                        sqlProjection.Name = projection.Alias;
                        sqlProjection.Table = tableExpression2.Name;
                        Reporter.WriteVerbose("project join table : " + sqlProjection.Table);
                    }
                    else
                    {
                        throw new Exception("projection type not defined...");
                    }
                }
                projections.Add(sqlProjection);
            }
            return projections;
        }


        private SelectExpression GetSelectExpression(IRelationalQueryingEnumerable relationalQueryingEnumerable)
        {
            var fieldInfos = relationalQueryingEnumerable.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var queryCommandCache = fieldInfos.FirstOrDefault(x => x.Name.Equals("_relationalCommandCache") && x.FieldType == typeof(RelationalCommandCache));
            if (queryCommandCache != null)
            {
                var relationalCommandCache = (RelationalCommandCache)queryCommandCache.GetValue(relationalQueryingEnumerable);
                fieldInfos = relationalCommandCache.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                var selectExpressionInfo = fieldInfos.FirstOrDefault(x => x.Name.Equals("_selectExpression") && x.FieldType == typeof(SelectExpression));
                var relationalParameterBasedSqlProcessorInfo = fieldInfos.FirstOrDefault(x => x.Name.Equals("_relationalParameterBasedSqlProcessor") && x.FieldType == typeof(RelationalParameterBasedSqlProcessor));
                if (selectExpressionInfo != null && relationalParameterBasedSqlProcessorInfo != null)
                {
                    var relationalParameterBasedSqlProcessor = (RelationalParameterBasedSqlProcessor)relationalParameterBasedSqlProcessorInfo.GetValue(relationalCommandCache);
                    var selectExpression = (SelectExpression)selectExpressionInfo.GetValue(relationalCommandCache);
                    return selectExpression;
                }
            }
            return null;
        }

        private IEnumerable<Type> GetQueryConfigurationTypes()
        {
            var type = typeof(IQueryGenerator<>);
            var queryGenerators = assembly.GetTypes().Where(x => !x.IsInterface &&
                x.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()));
            return queryGenerators;
        }

        private Type GetDbContextType(Type queryGenerator)
        {
            var type = typeof(IQueryGenerator<>);
            var contextType = queryGenerator.GetInterfaces().First(x => x.IsGenericType &&
                x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()).GetGenericArguments()
                .First();
            return contextType;
        }


        private (Language, IOperationGenerator) GetLanguageGenerator()
        {
            var dependencies = new SqlOperationGeneratorDependencies(new SQLGenerator(_sqlGenerationHelper), _relationalTypeMappingSource);
            if (_queryOptions.Language.Equals(Language.CSharp))
            {
                return (Language.CSharp, new CsharpOperationGenerator(dependencies, _queryOptions, Language.CSharp, _tables, _sqlModel));
            }
            else if (_queryOptions.Language.Equals(Language.Go))
            {
                return (Language.Go, new GoOperationGenerator(dependencies, _queryOptions, Language.Go, _tables, _sqlModel));
            }
            else
            {
                throw new Exception("Language is not currently supported...");
            }
        }
    }
}
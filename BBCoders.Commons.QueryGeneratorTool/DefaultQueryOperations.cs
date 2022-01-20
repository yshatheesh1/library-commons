using System;
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
using Humanizer;

namespace BBCoders.Commons.QueryGeneratorTool
{
    public class DefaultQueryOperations : QueryGenerator.QueryContext
    {
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;
        private IRelationalModel _model;
        private ISqlGenerationHelper _sqlGenerationHelper;
        private IRelationalTypeMappingSource _relationalTypeMappingSource;
        private IOperationReporter _reporter;
        private DbContext _dbContext;
        private List<QueryModel> _sqlModel;
        private List<ITable> _tables;
        private QueryOptions _queryOptions;
        public DefaultQueryOperations([NotNull] Assembly assembly, [NotNull] Assembly startupAssembly, [NotNull] string projectDir, [NotNull] string rootNamespace, [NotNull] string[] designArgs) :
         base(assembly, startupAssembly, projectDir, rootNamespace, designArgs)
        {
            var handler = new OperationReportHandler(
                    (Action<string>)Reporter.WriteError,
                    (Action<string>)Reporter.WriteWarning,
                    (Action<string>)Reporter.WriteInformation,
                    (Action<string>)Reporter.WriteVerbose);
            _reporter = new OperationReporter(handler);
            _contextOperations = new DbContextOperations(_reporter, assembly, startupAssembly, projectDir, rootNamespace, "C#", false, designArgs);
            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, _reporter, designArgs);
        }

        /// <summary>
        /// execute all query configurations using reflection
        /// </summary>
        public void Execute()
        {
            _reporter.WriteInformation("Started generating source for queries");
            var type = typeof(IQueryGenerator<>);
            var queryGenerators = GetQueryConfigurationTypes();
            _reporter.WriteInformation($"Found {queryGenerators.Count()} query generators.");
            foreach (var queryGenerator in queryGenerators)
            {
                var contextType = queryGenerator.GetInterfaces().First(x => x.IsGenericType &&
                x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()).GetGenericArguments()
                .First().Name;
                _dbContext = _contextOperations.CreateContext(contextType);
                var services = _servicesBuilder.Build(_dbContext);
                using (var scope = services.CreateScope())
                {
                    _relationalTypeMappingSource = scope.ServiceProvider.GetService<IRelationalTypeMappingSource>();
                    _sqlGenerationHelper = scope.ServiceProvider.GetService<ISqlGenerationHelper>();
                    var migrationsScaffolderDependencies = scope.ServiceProvider.GetService<MigrationsScaffolderDependencies>();
                    _model = migrationsScaffolderDependencies.Model.GetRelationalModel();
                    var queryGeneratorInstance = Activator.CreateInstance(queryGenerator, new object[] { });
                    var queryConfigMethod = queryGenerator.GetMethod("GetQueryOptions");
                    _queryOptions = (QueryOptions)queryConfigMethod.Invoke(queryGeneratorInstance, new object[] { });
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
                    // operationGenerators = new List<IOperationGenerator>();
                    _tables = new List<ITable>();
                    _sqlModel = new List<QueryModel>();
                    var createQuery = queryGenerator.GetMethod("CreateQuery");
                    createQuery.Invoke(queryGeneratorInstance, new object[] { _dbContext, this });
                    GenerateCode();
                }
            }
        }


        /// <summary>
        ///  adds default methods for a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Add<T>()
        {
            var table = _model?.Tables.FirstOrDefault(x => x.EntityTypeMappings.First().EntityType.ClrType == typeof(T));
            if (table == null)
            {
                var error = $"Table with given type not defined -{typeof(T)}. Please add this to DbContext if not added.";
                Reporter.WriteError(error);
                throw new Exception(error);
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
            var customSqlModel = new QueryModel(name);
            var parameters = ParameterExpressionHelper.GetParameters(expression.Body.ToString(), inputParameters);
            var func = expression.Compile();
            var bindingDefaultValues = parameters.Item2;
            var bindingParameters = parameters.Item1.Values.ToArray();
            var query = (IQueryable)func.GetType().GetMethod("Invoke").Invoke(func, bindingDefaultValues);
            if (query.Provider.Execute<IEnumerable>(query.Expression) is IRelationalQueryingEnumerable queryingEnumerable)
            {
                var projections = CreateProjections(queryingEnumerable);
                customSqlModel.Projections.AddRange(projections);
                var command = queryingEnumerable.CreateDbCommand();
                customSqlModel.Sql = command.CommandText.Replace("\n", "\n\t\t\t\t");
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
                        var placeholder = binding.Expression.Name.Pluralize() + "Joined";
                        var inClause = $" IN (\" + {placeholder} + @\")";
                        customSqlModel.Sql = customSqlModel.Sql.Replace(match.Value, inClause);
                        index = match.Index + inClause.Length;
                        // inputParameter.ListPlaceholder = placeholder;
                    }
                    customSqlModel.Parameters.Add(inputParameter);
                    customSqlModel.Bindings.Add(new Binding()
                    {
                        Name = binding.Expression.Name,
                        Value = inputParameter
                    });
                }
            }
            _sqlModel.Add(customSqlModel);
        }

        private Type GetDbType(Type type)
        {
            var relationalTypeMapping = _relationalTypeMappingSource.GetMapping(type);
            return GetDbType(relationalTypeMapping);
        }

        private Type GetDbType(RelationalTypeMapping relationalTypeMapping)
        {
            var type = relationalTypeMapping.DbType.HasValue ? SqlMapperHelper.getClrType(relationalTypeMapping.DbType.Value) :
                    relationalTypeMapping.ClrType;
            return type;
        }

        private List<Projection> CreateProjections(IRelationalQueryingEnumerable queryingEnumerable)
        {
            var projections = new List<Projection>();
            var constantPropertyIndex = 0;
            var selectExpression = GetSelectExpression(queryingEnumerable);
            foreach (var projection in selectExpression.Projection)
            {
                var type = GetDbType(projection.Expression.TypeMapping);
                var sqlProjection = new Projection()
                {
                    Name = projection.Alias,
                    Type = type
                };
                // if no name is provided for custom sql 
                if (string.IsNullOrEmpty(sqlProjection.Name))
                {
                    sqlProjection.Name = $"Value_{constantPropertyIndex}";
                    constantPropertyIndex++;
                }
                if (projection.Expression is ColumnExpression columnExpression)
                {
                    sqlProjection.IsNullable = columnExpression.IsNullable;
                    if (columnExpression.Table is TableExpression tableExpression)
                    {
                        sqlProjection.Name = projection.Alias;
                        sqlProjection.Table = tableExpression.Name;
                    }
                    if (columnExpression.Table is JoinExpressionBase joinExpressionBase && joinExpressionBase.Table is TableExpression tableExpression2)
                    {
                        sqlProjection.Name = projection.Alias;
                        sqlProjection.Table = tableExpression2.Name;
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

        private void GenerateCode()
        {
            var dependencies = new SqlOperationGeneratorDependencies(new SQLGenerator(_sqlGenerationHelper), _relationalTypeMappingSource);
            if (_queryOptions.Language.Equals(Language.CSharp))
            {
                new CsharpOperationGenerator(dependencies, _queryOptions, Language.CSharp, _tables, _sqlModel).Generate();
            }
            else if (_queryOptions.Language.Equals(Language.Go))
            {
                new GoOperationGenerator(dependencies, _queryOptions, Language.Go, _tables, _sqlModel).Generate();
            }
            else
            {
                throw new Exception("Language is not currently supported...");
            }
        }
    }
}
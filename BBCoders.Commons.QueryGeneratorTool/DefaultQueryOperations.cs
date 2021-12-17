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
using Humanizer;
using BBCoders.Commons.QueryGenerator;
using BBCoders.Commons.QueryGeneratorTool.Helpers;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics;
using System.Threading;

namespace BBCoders.Commons.QueryGeneratorTool
{
    public class DefaultQueryOperations : QueryGenerator.QueryContext
    {
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;
        private IRelationalModel model;
        private ISqlGenerationHelper sqlGenerationHelper;
        private List<ICodeGenerator> codeGenerators = new List<ICodeGenerator>();
        private List<IOperationGenerator> operationGenerators;
        private IRelationalTypeMappingSource relationalTypeMappingSource;
        private IOperationReporter _reporter;
        private DbContext _dbContext;
        public DefaultQueryOperations([NotNull] Assembly assembly, [NotNull] Assembly startupAssembly, [NotNull] string projectDir, [NotNull] string rootNamespace, [NotNull] string[] designArgs) :
         base(assembly, startupAssembly, projectDir, rootNamespace, designArgs)
        {
            var handler = new OperationReportHandler(
                    (Action<string>)Reporter.WriteError,
                    (Action<string>)Reporter.WriteWarning,
                    (Action<string>)Reporter.WriteInformation,
                    (Action<string>)Reporter.WriteVerbose);
            _reporter = new OperationReporter(handler);
            _contextOperations = new DbContextOperations(_reporter, assembly, startupAssembly, designArgs);
            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, _reporter, designArgs);
        }

        /// <summary>
        /// execute all query configurations using reflection
        /// </summary>
        public void Execute()
        {
            var type = typeof(IQueryGenerator<>);
            var queryGenerators = assembly.GetTypes().Where(x => !x.IsInterface &&
                x.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()));
            foreach (var queryGenerator in queryGenerators)
            {
                var contextType = queryGenerator.GetInterfaces().First(x => x.IsGenericType &&
                x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()).GetGenericArguments()
                .First().Name;
                _dbContext = _contextOperations.CreateContext(contextType);
                var services = _servicesBuilder.Build(_dbContext);
                using (var scope = services.CreateScope())
                {
                    relationalTypeMappingSource = scope.ServiceProvider.GetService<IRelationalTypeMappingSource>();
                    sqlGenerationHelper = scope.ServiceProvider.GetService<ISqlGenerationHelper>();
                    var migrationsScaffolderDependencies = scope.ServiceProvider.GetService<MigrationsScaffolderDependencies>();
                    model = migrationsScaffolderDependencies.Model.GetRelationalModel();
                    var queryGeneratorInstance = Activator.CreateInstance(queryGenerator, new object[] { });
                    var queryConfigMethod = queryGenerator.GetMethod("GetQueryOptions");
                    var queryOptions = (QueryOptions)queryConfigMethod.Invoke(queryGeneratorInstance, new object[] { });
                    if (queryOptions == null)
                    {
                        throw new ArgumentNullException(nameof(queryOptions));
                    }
                    operationGenerators = new List<IOperationGenerator>();
                    var createQuery = queryGenerator.GetMethod("CreateQuery");
                    createQuery.Invoke(queryGeneratorInstance, new object[] { _dbContext, this });
                    codeGenerators.Add(new DefaultCodeGenerator(queryOptions, operationGenerators));
                }
            }
            codeGenerators.ForEach(x => x.Generate());
        }
        /// <summary>
        ///  adds default methods for a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Add<T>()
        {
            var table = model?.Tables.FirstOrDefault(x => x.EntityTypeMappings.First().EntityType.ClrType == typeof(T));
            if (table == null)
            {
                var error = $"Table with given type not defined -{typeof(T)}. Please add this to DbContext if not added.";
                Reporter.WriteError(error);
                throw new Exception(error);
            }
            var dependencies = new SqlOperationGeneratorDependencies(sqlGenerationHelper, relationalTypeMappingSource);
            operationGenerators.Add(new SelectSqlOperationGenerator(dependencies, table));
            operationGenerators.Add(new InsertSqlOperationGenerator(dependencies, table));
            operationGenerators.Add(new UpdateSqlOperationGenerator(dependencies, table));
            operationGenerators.Add(new DeleteSqlOperationGenerator(dependencies, table));
        }

        public override void Add<T>(string name, List<ParameterExpression> inputParameters, Expression<T> expression)
        {
            var constantPropertyIndex = 0;
            var customSqlModel = new SqlModel(name);
            var parameters = ParameterExpressionHelper.GetParameters(expression.Body.ToString(), inputParameters);
            var func = expression.Compile();
            var bindingDefaultValues = parameters.Item2;
            var bindingParameters = parameters.Item1.Values.ToArray();
            var query = (IQueryable)func.GetType().GetMethod("Invoke").Invoke(func, bindingDefaultValues);
            if (query.Provider.Execute<IEnumerable>(query.Expression) is IRelationalQueryingEnumerable queryingEnumerable)
            {
                var selectExpression = GetSelectExpression(queryingEnumerable);
                foreach (var projection in selectExpression.Projection)
                {
                    var type = GetDbType(projection.Expression.TypeMapping);
                    var sqlProjection = new SqlProjection()
                    {
                        IsValueType = type.IsValueType,
                        Name = projection.Alias,
                        Type = type.Name,
                    };
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
                            sqlProjection.Name = tableExpression.Name.Singularize() + projection.Alias;
                        }
                    }
                    customSqlModel.Projections.Add(sqlProjection);
                }
                var command = queryingEnumerable.CreateDbCommand();
                customSqlModel.Sql = command.CommandText;
                var equalParameters = bindingParameters.Where(x => !x.InExpression).ToList();
                for (var i = 0; i < command.Parameters.Count; i++)
                {
                    var parameter = command.Parameters[i];
                    var bindingParameter = equalParameters.Count() > i ? equalParameters[i] : null;
                    // if user provides default value in the query, no user input is required
                    if (bindingParameter == null)
                    {
                        customSqlModel.EqualBindings.Add(new SqlBinding()
                        {
                            Name = parameter.ParameterName,
                            hasDefault = true,
                            DefaultValue = parameter.Value
                        });
                    }
                    else
                    {
                        var type = GetDbType(bindingParameter.Expression.Type);
                        customSqlModel.EqualBindings.Add(new SqlBinding()
                        {
                            Name = parameter.ParameterName,
                            Type = type.Name,
                            Value = bindingParameter.Expression.Name,
                        });
                    }
                }
                foreach (var binding in bindingParameters.Where(x => x.InExpression))
                {
                    var type = GetDbType(binding.Expression.Type.GetGenericArguments()[0]);
                    customSqlModel.InBindings.Add(new SqlBinding()
                    {
                        Name = binding.Expression.Name,
                        Type = $"List<{type.Name}>",
                        Value = binding.Expression.Name
                    });
                }
            }
            operationGenerators.Add(new CustomSqlOperationGenerator(customSqlModel));
        }

        private Type GetDbType(Type type)
        {
            var relationalTypeMapping = relationalTypeMappingSource.GetMapping(type);
            return GetDbType(relationalTypeMapping);
        }

        private Type GetDbType(RelationalTypeMapping relationalTypeMapping)
        {
            var type = relationalTypeMapping.DbType.HasValue ? SqlMapperHelper.getClrType(relationalTypeMapping.DbType.Value) :
                    relationalTypeMapping.ClrType;
            return type;
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
    }
}
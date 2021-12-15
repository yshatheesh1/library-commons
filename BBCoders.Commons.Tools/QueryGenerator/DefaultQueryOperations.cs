using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BBCoders.Commons.QueryConfiguration;
using BBCoders.Commons.Tools.QueryGenerator.Models;
using BBCoders.Commons.Tools.QueryGenerator.Services;
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
using BBCoders.Commons.Tools.QueryGenerator.Helpers;
using Humanizer;

namespace BBCoders.Commons.Tools.QueryGenerator
{
    public class DefaultQueryOperations : QueryOperations
    {
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;
        private IRelationalModel model;
        private ISqlGenerationHelper sqlGenerationHelper;
        private List<ICodeGenerator> codeGenerators = new List<ICodeGenerator>();
        private List<IOperationGenerator> operationGenerators;
        private IRelationalTypeMappingSource relationalTypeMappingSource;
        private IOperationReporter _reporter;
        public DefaultQueryOperations([NotNull] IOperationReporter reporter, [NotNull] Assembly assembly, [NotNull] Assembly startupAssembly, [NotNull] string projectDir, [NotNull] string rootNamespace, [NotNull] string[] designArgs) :
         base(assembly, startupAssembly, projectDir, rootNamespace, designArgs)
        {
            _reporter = reporter;
            _contextOperations = new DbContextOperations(reporter, assembly, startupAssembly, designArgs);
            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, designArgs);
        }

        /// <summary>
        /// execute all query configurations using reflection
        /// </summary>
        public void Execute()
        {
            var type = typeof(IQueryConfiguration<>);
            var queryConfigurations = assembly.GetTypes().Where(x => !x.IsInterface &&
                x.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()));
            foreach (var queryConfiguration in queryConfigurations)
            {
                var contextType = queryConfiguration.GetInterfaces().First(x => x.IsGenericType &&
                x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()).GetGenericArguments()
                .First().Name;
                using (var context = _contextOperations.CreateContext(contextType))
                {
                    var services = _servicesBuilder.Build(context);
                    using (var scope = services.CreateScope())
                    {
                        relationalTypeMappingSource = scope.ServiceProvider.GetService<IRelationalTypeMappingSource>();
                        sqlGenerationHelper = scope.ServiceProvider.GetService<ISqlGenerationHelper>();
                        var migrationsScaffolderDependencies = scope.ServiceProvider.GetService<MigrationsScaffolderDependencies>();
                        model = migrationsScaffolderDependencies.Model.GetRelationalModel();
                        var queryConfigurationInstance = Activator.CreateInstance(queryConfiguration, new object[] { });
                        var queryConfigMethod = queryConfiguration.GetMethod("GetQueryOptions");
                        var queryOptions = (QueryOptions)queryConfigMethod.Invoke(queryConfigurationInstance, new object[] { });
                        if (queryOptions == null)
                        {
                            throw new ArgumentNullException(nameof(queryOptions));
                        }
                        operationGenerators = new List<IOperationGenerator>();
                        var createQuery = queryConfiguration.GetMethod("CreateQuery");
                        createQuery.Invoke(queryConfigurationInstance, new object[] { context, this });
                        codeGenerators.Add(new DefaultCodeGenerator(queryOptions, operationGenerators));
                    }
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
            var customSqlModel = new SqlModel(name);
            var parameters = ParameterExpressionHelper.GetParameters(expression.Body.ToString(), inputParameters);
            var func = expression.Compile();
            var query = (IQueryable)func.GetType().GetMethod("Invoke").Invoke(func, parameters.Item2);
            if (query.Provider.Execute<IEnumerable>(query.Expression) is IRelationalQueryingEnumerable queryingEnumerable)
            {
                var command = queryingEnumerable.CreateDbCommand();
                customSqlModel.Sql = command.CommandText;
                var fieldInfos = queryingEnumerable.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                var queryCommandCache = fieldInfos.FirstOrDefault(x => x.Name.Equals("_relationalCommandCache") && x.FieldType == typeof(RelationalCommandCache));
                if (queryCommandCache != null)
                {
                    var relationalCommandCache = (RelationalCommandCache)queryCommandCache.GetValue(queryingEnumerable);
                    fieldInfos = relationalCommandCache.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    var selectExpressionInfo = fieldInfos.FirstOrDefault(x => x.Name.Equals("_selectExpression") && x.FieldType == typeof(SelectExpression));
                    var relationalParameterBasedSqlProcessorInfo = fieldInfos.FirstOrDefault(x => x.Name.Equals("_relationalParameterBasedSqlProcessor") && x.FieldType == typeof(RelationalParameterBasedSqlProcessor));
                    if (selectExpressionInfo != null && relationalParameterBasedSqlProcessorInfo != null)
                    {
                        var relationalParameterBasedSqlProcessor = (RelationalParameterBasedSqlProcessor)relationalParameterBasedSqlProcessorInfo.GetValue(relationalCommandCache);
                        var selectExpression = (SelectExpression)selectExpressionInfo.GetValue(relationalCommandCache);
                        foreach (var projection in selectExpression.Projection)
                        {
                            var columnExpression = (ColumnExpression)projection.Expression;
                            var tableExpression = (TableExpression)columnExpression.Table;
                            var relMapping = columnExpression.TypeMapping;
                            var sqlProjection = new SqlProjection()
                            {
                                IsNullable = columnExpression.IsNullable,
                                Name = tableExpression.Name.Singularize() + projection.Alias,
                                Value = projection.Alias,
                                Type = relMapping.DbType.HasValue ? SqlMapperHelper.getClrType(relMapping.DbType.Value).Name : relMapping.ClrType.Name,
                            };
                            customSqlModel.Projections.Add(sqlProjection);
                        }
                    }
                }
                var sortedParameters = parameters.Item1.Values.ToArray();
                var equalParameters = sortedParameters.Where(x => !x.InExpression).ToList();
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
                        var relMapping = relationalTypeMappingSource.GetMapping(bindingParameter.Expression.Type);
                        customSqlModel.EqualBindings.Add(new SqlBinding()
                        {
                            Name = parameter.ParameterName,
                            Type = relMapping.DbType.HasValue ? SqlMapperHelper.getClrType(relMapping.DbType.Value).Name : relMapping.ClrType.Name,
                            Value = bindingParameter.Expression.Name,
                        });
                    }
                }
                foreach (var binding in sortedParameters.Where(x => x.InExpression))
                {
                    var relMapping = relationalTypeMappingSource.GetMapping(binding.Expression.Type.GetGenericArguments()[0]);
                    var type = relMapping.DbType.HasValue ? SqlMapperHelper.getClrType(relMapping.DbType.Value).Name : relMapping.ClrType.Name;
                    customSqlModel.InBindings.Add(new SqlBinding()
                    {
                        Name = binding.Expression.Name,
                        Type = $"List<{type}>",
                        Value = binding.Expression.Name
                    });
                }
            }
            operationGenerators.Add(new CustomSqlOperationGenerator(customSqlModel));
        }
    }
}
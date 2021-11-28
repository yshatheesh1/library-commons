using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
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
using BBCoders.Commons.Tools.src.QueryGenerator.Helpers;
using BBCoders.Commons.Tools.src.QueryGenerator.Services;
using System.Diagnostics;
using System.Threading;

namespace BBCoders.Commons.Tools.QueryGenerator
{
    public class DefaultQueryOperations : QueryOperations
    {
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;
        private IRelationalModel model;
        private ISqlGenerationHelper sqlGenerationHelper;
        private List<ICodeGenerator> codeGenerators = new List<ICodeGenerator>();
        private List<ISqlOperationGenerator> operationGenerators;
        private IOperationReporter _reporter;
        public DefaultQueryOperations([NotNull] IOperationReporter reporter, [NotNull] Assembly assembly, [NotNull] Assembly startupAssembly, [NotNull] string projectDir, [NotNull] string rootNamespace, [NotNull] string[] designArgs) :
         base(assembly, startupAssembly, projectDir, rootNamespace, designArgs)
        {
            _reporter = reporter;
            _contextOperations = new DbContextOperations(reporter, assembly, startupAssembly, designArgs);
            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, designArgs);
        }

        public void Execute()
        {
            // while(!Debugger.IsAttached) {
            //     Thread.Sleep(100);
            // }
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
                        operationGenerators = new List<ISqlOperationGenerator>();
                        var createQuery = queryConfiguration.GetMethod("CreateQuery");
                        createQuery.Invoke(queryConfigurationInstance, new object[] { context, this });
                        codeGenerators.Add(new DefaultCodeGenerator(queryOptions, operationGenerators));
                    }
                }
            }
            codeGenerators.ForEach(x => x.Generate());
        }
        public override void Add<T>(string selectMethodName, string insertMethodName, string updateMethodName, string deleteMethodName)
        {
            var table = model?.Tables.FirstOrDefault(x => x.EntityTypeMappings.First().EntityType.ClrType == typeof(T));
            if (table == null)
            {
                var error = $"Table with given type not defined -{typeof(T)}. Please add this to DbContext if not added.";
                Reporter.WriteError(error);
                throw new Exception(error);
            }
            operationGenerators.Add(new SelectSqlOperationGenerator(sqlGenerationHelper, table));
            operationGenerators.Add(new InsertSqlOperationGenerator(sqlGenerationHelper, table));
            operationGenerators.Add(new UpdateSqlOperationGenerator(sqlGenerationHelper, table));
            operationGenerators.Add(new DeleteSqlOperationGenerator(sqlGenerationHelper, table));
        }

        public override void Add<T>(string name, List<ParameterExpression> inputParameters, Expression<T> expression)
        {
            var customSqlModel = new SqlModel() { MethodName = name, Projections = new List<SqlProjection>(), Bindings = new List<SqlBinding>() };

            var parameters = ParameterExpressionHelper.GetParameters(expression.Body.ToString(), inputParameters);
            var func = expression.Compile();
            var test = (IQueryable)func.GetType().GetMethod("Invoke").Invoke(func, parameters.Item2);
            if (test.Provider.Execute<IEnumerable>(test.Expression) is IRelationalQueryingEnumerable queryingEnumerable)
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
                        foreach (var projection in selectExpression.Projection.GroupBy(x => x.Alias).Select(x => new { Key = x.Key, Value = x.First() }))
                        {
                            var sqlProjection = new SqlProjection()
                            {
                                Name = projection.Key,
                                Type = projection.Value.Type.Name,
                            };
                            customSqlModel.Projections.Add(sqlProjection);
                        }
                    }
                }
                var sortedParameters = parameters.Item1.Values.ToArray();
                for (var i = 0; i < command.Parameters.Count; i++)
                {
                    var parameter = command.Parameters[i];
                    var bindingParameter = sortedParameters.Count() > i ? sortedParameters[i] : null;
                    var sqlBinding = new SqlBinding()
                    {
                        Name = parameter.ParameterName,
                        Type = bindingParameter?.Type.Name,
                        Value = bindingParameter?.Name,
                        hasDefault = bindingParameter == null,
                        DefaultValue = parameter.Value
                    };
                    customSqlModel.Bindings.Add(sqlBinding);
                }
            }
            operationGenerators.Add(new CustomSqlOperationGenerator(customSqlModel));
        }
    }
}
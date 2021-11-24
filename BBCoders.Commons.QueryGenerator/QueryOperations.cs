
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BBCoders.Commons.QueryGenerator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace BBCoders.Commons.QueryGenerator
{
    public class QueryOperations
    {
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;
        private IOperationReporter reporter;
        private Assembly assembly;
        private Assembly startupAssembly;
        private string projectDir;
        private string rootNamespace;
        private string language;
        private bool nullable;
        private string[] designArgs;
        private IRelationalModel model;
        private ISqlGenerationHelper sqlGenerationHelper;
        private List<ICodeGenerator> codeGenerators = new List<ICodeGenerator>();

        internal QueryOperations(IOperationReporter reporter, Assembly assembly, Assembly startupAssembly, string projectDir, string rootNamespace, string language, bool nullable, string[] designArgs)
        {
            this.reporter = reporter;
            this.assembly = assembly;
            this.startupAssembly = startupAssembly;
            this.projectDir = projectDir;
            this.rootNamespace = rootNamespace;
            this.language = language;
            this.nullable = nullable;
            this.designArgs = designArgs;

            _contextOperations = new DbContextOperations(reporter, assembly, startupAssembly, designArgs);
            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, designArgs);
            Execute();
        }

        private void Execute()
        {
            var type = typeof(IQueryConfiguration<>);
            var queryConfigurations = assembly.GetTypes().Where(x => !x.IsInterface &&
                x.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()));
            using var context = _contextOperations.CreateContext(null);
            var services = _servicesBuilder.Build(context);
            using var scope = services.CreateScope();
            sqlGenerationHelper = scope.ServiceProvider.GetService<ISqlGenerationHelper>();
            var migrationsScaffolderDependencies = scope.ServiceProvider.GetService<MigrationsScaffolderDependencies>();
            model = migrationsScaffolderDependencies.Model.GetRelationalModel();
            foreach (var queryConfiguration in queryConfigurations)
            {
                var queryConfigurationInstance = Activator.CreateInstance(queryConfiguration, new object[] { });
                var method = queryConfiguration.GetMethod("CreateQuery");
                method.Invoke(queryConfigurationInstance, new object[] {
                        context,
                        this
                    });
            }
            QueryGenerator.GenerateCode(rootNamespace, codeGenerators);
        }

        public void Add<T>()
        {
            var table = model?.Tables.FirstOrDefault(x => x.EntityTypeMappings.First().EntityType.ClrType == typeof(T));
            if (table == null)
            {
                var error = $"Table with given type not defined -{typeof(T)}. Please add this to DbContext if not added.";
                reporter.WriteError(error);
                throw new Exception(error);
            }
            codeGenerators.Add(new SelectSqlOperationGenerator(sqlGenerationHelper, table));
            codeGenerators.Add(new InsertSqlOperationGenerator(sqlGenerationHelper, table));
            codeGenerators.Add(new UpdateSqlOperationGenerator(sqlGenerationHelper, table));
            codeGenerators.Add(new DeleteSqlOperationGenerator(sqlGenerationHelper, table));
        }
        public void Add<T>(string name, List<ParameterExpression> inputParameters, Expression<T> expression)
        {
            var customSqlModel = new CustomSqlModel() { MethodName = name, Projections = new List<SqlProjection>(), Bindings = new List<SqlBinding>() };

            var parameters = GetParameters(inputParameters);
            var func = expression.Compile();
            var test = (IQueryable)func.GetType().GetMethod("Invoke").Invoke(func, parameters.Keys.ToArray());
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
                foreach (var pt in command.Parameters.Cast<DbParameter>())
                {
                    var sqlBinding = new SqlBinding()
                    {
                        Name = pt.ParameterName,
                        Type = parameters[pt.Value].Type.Name,
                        Value = parameters[pt.Value].Name
                    };
                    customSqlModel.Bindings.Add(sqlBinding);
                }
            }
            codeGenerators.Add(new CustomSqlOperationGenerator(customSqlModel));
        }
        private static Dictionary<object, ParameterExpression> GetParameters(List<ParameterExpression> parameters)
        {
            var bindings = new Dictionary<object, ParameterExpression>();
            for (var i = 0; i < parameters.Count; i++)
            {
                if (!parameters[i].Type.IsValueType)
                {
                    throw new Exception("only value types are allowed for binding");
                }
                if (parameters[i].Type == typeof(System.String))
                {
                    bindings.Add("RandomString", parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Guid))
                {
                    bindings.Add(Guid.NewGuid(), parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.DateTime))
                {
                    bindings.Add(DateTime.Now, parameters[i]);
                }

                else if (parameters[i].Type == typeof(System.Boolean))
                {
                    bindings.Add(true, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Byte))
                {
                    bindings.Add(Byte.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.SByte))
                {
                    bindings.Add(SByte.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Char))
                {
                    bindings.Add(Char.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Decimal))
                {
                    bindings.Add(Decimal.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Double))
                {
                    bindings.Add(Double.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Single))
                {
                    bindings.Add(Single.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Int16))
                {
                    bindings.Add(Int16.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.UInt16))
                {
                    bindings.Add(UInt16.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.IntPtr))
                {
                    bindings.Add(IntPtr.Zero, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.UIntPtr))
                {
                    bindings.Add(UIntPtr.Zero, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Int32))
                {
                    bindings.Add(Int32.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.UInt32))
                {
                    bindings.Add(UInt32.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.Int64))
                {
                    bindings.Add(Int64.MaxValue, parameters[i]);
                }
                else if (parameters[i].Type == typeof(System.UInt64))
                {
                    bindings.Add(UInt64.MaxValue, parameters[i]);
                }
                else
                {
                    throw new Exception("Given type of parameter is not value type - " + parameters[i].Type);
                }
            }
            return bindings;
        }

    }
}
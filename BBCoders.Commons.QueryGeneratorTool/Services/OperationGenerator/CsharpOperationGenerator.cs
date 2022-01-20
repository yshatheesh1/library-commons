using System.Collections.Generic;
using System.Linq;
using BBCoders.Commons.QueryGenerator;
using BBCoders.Commons.QueryGeneratorTool.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public class CsharpOperationGenerator : BaseOperationGenerator
    {
        public CsharpOperationGenerator(SqlOperationGeneratorDependencies dependencies, QueryOptions options, Language language, List<ITable> tables, List<QueryModel> sqlModels) : base(dependencies, options, language, tables, sqlModels)
        {
        }

        protected override void GenerateModel(IndentedStringBuilder builder, ClassModel classModel)
        {
            GenerateComment(builder);
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine();
            builder.AppendLine($"namespace {_queryOptions.ModelPackageName}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                GenerateModel(builder, classModel.Name, classModel.Properties, classModel.NestedClass);
            }
            builder.AppendLine("}");
        }

        private void GenerateModel(IndentedStringBuilder builder, string className, List<PropertyModel> properties, List<ClassModel> nestedClasses)
        {
            builder.AppendLine($"public class {className}");
            builder.AppendLine("{");
            using (builder.Indent())
            {

                foreach (var property in properties)
                {
                    var typeName = _language.Type[property.CSharpType];
                    typeName = property.IsNullable && property.IsValueType ? typeName + "?" : typeName;
                    if (property.IsList)
                    {
                        typeName = "List<" + typeName + ">";
                    }
                    builder.Append($"public {typeName} {property.Name}")
                            .AppendLine(" { get; set; }");
                }

                builder.AppendLine($"public {className}()");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    foreach (var nestedModel in nestedClasses)
                    {
                        var nestedClassTypeName = nestedModel.Name + "Model";
                        builder.Append($"{nestedModel.Name}")
                        .AppendLine($" = new {nestedClassTypeName}();");
                    }
                }
                builder.AppendLine("}");

                foreach (var nestedModel in nestedClasses)
                {
                    var nestedClassTypeName = nestedModel.Name + "Model";
                    builder.Append($"public {nestedClassTypeName} {nestedModel.Name}")
                             .AppendLine(" { get; set; }");
                    GenerateModel(builder, nestedClassTypeName, nestedModel.Properties, nestedModel.NestedClass);
                }
            }
            builder.AppendLine("}");
        }

        protected override void GenerateMethods(IndentedStringBuilder builder, List<MethodOperation> methodOperations)
        {
            GenerateComment(builder);
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using System.Threading.Tasks;");
            builder.AppendLine("using System.Data;");
            builder.AppendLine("using System.Data.Common;");
            builder.AppendLine("using System.Text;");
            if (!_queryOptions.PackageName.Equals(_queryOptions.ModelPackageName))
            {
                builder.AppendLine($"using {_queryOptions.ModelPackageName};");
            }
            builder.AppendLine();
            builder.AppendLine($"namespace {_queryOptions.PackageName}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine($"public static class {_queryOptions.ClassName}");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    foreach (var methodOperation in methodOperations)
                    {
                        GenerateMethod(builder, methodOperation);
                    }
                    GenerateHelperMethods(builder);
                }
                builder.AppendLine("}");
            }

            builder.AppendLine("}");
        }

        public void GenerateMethod(IndentedStringBuilder builder, MethodOperation methodOperation)
        {
            var methodName = methodOperation.MethodName;
            var requestModelName = methodOperation.InputModelName;
            var responseModelName = methodOperation.OutputModelName;
            var inputType = methodOperation.IsBatchOperation ? $"List<{requestModelName}> {requestModelName}" : $"{requestModelName} {requestModelName}";
            var returnType = methodOperation.HasResult ? (methodOperation.IsBatchOperation || methodOperation.SqlType == SqlType.Custom) ? $"List<{responseModelName}>" : responseModelName : "int";
            builder.AppendLine($"public static async Task<{returnType}> {methodName}(this DbConnection connection, {inputType}, DbTransaction transaction = null, int? timeout = null)");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                var whereMappings = new List<string>();
                var inMappings = new List<string>();
                foreach (var property in methodOperation.InputModel)
                {
                    if (property.IsPrimaryKey && methodOperation.IsBatchOperation && (methodOperation.SqlType == SqlType.Select || methodOperation.SqlType == SqlType.Delete))
                    {
                        var propertyName = $"{property.PropertyName.Pluralize()}Joined";
                        builder.AppendLine($"var {propertyName} = string.Join(\",\", {requestModelName}.Select((_, idx) => \"@{property.ColumnName}\" + idx));");
                        whereMappings.Add(propertyName);
                    }
                    if (property.IsListType)
                    {
                        var propertyName = $"{property.PropertyName.Pluralize()}Joined";
                        builder.AppendLine($"var {propertyName} = string.Join(\",\", {requestModelName}?.{property.ColumnName}.Select((x, idx) => \"@{property.ColumnName}\" + idx));");
                        inMappings.Add(propertyName);
                    }
                }

                // sql
                if (methodOperation.SqlType == SqlType.Custom)
                {
                    builder.AppendLine($"var sql = @\"{methodOperation.CustomSql}\";");
                }
                if (methodOperation.SqlType == SqlType.Select || methodOperation.SqlType == SqlType.Delete)
                {
                    CreateSelect_Delete(methodOperation, builder, whereMappings.ToArray());
                }
                if (methodOperation.SqlType == SqlType.Insert || methodOperation.SqlType == SqlType.Update)
                {
                    CreateInsert_Update(methodOperation, builder, requestModelName);
                }

                // command parameters
                CreateCommandParameters(methodOperation, builder, requestModelName);

                // create connection
                builder.AppendLine("if (connection.State == ConnectionState.Closed)");
                using (builder.Indent())
                {
                    builder.AppendLine("await connection.OpenAsync();");
                }

                if (!methodOperation.HasResult)
                {
                    builder.AppendLine("return await command.ExecuteNonQueryAsync();");
                }
                else
                {
                    builder.AppendLine($"var results = new List<{responseModelName}>();");
                    builder.AppendLine("var reader = await command.ExecuteReaderAsync();");
                    if (methodOperation.IsBatchOperation && (methodOperation.SqlType == SqlType.Insert || methodOperation.SqlType == SqlType.Update))
                    {
                        builder.AppendLine("while (await reader.ReadAsync() || (await reader.NextResultAsync() && await reader.ReadAsync()))");
                    }
                    else
                    {
                        builder.AppendLine("while (await reader.ReadAsync())");
                    }
                    builder.AppendLine("{");
                    using (builder.Indent())
                    {
                        builder.AppendLine($"var result = new {responseModelName}();");
                        for (var i = 0; i < methodOperation.OutputModel.Count; i++)
                        {
                            var parameter = methodOperation.OutputModel[i];
                            builder.Append($"result.{parameter.PropertyName} = ");
                            if (parameter.IsNullable)
                            {
                                builder.AppendLine($"Convert.IsDBNull(reader[{i}]) ? null : ({parameter.Type}?)reader[{i}];");
                            }
                            else
                            {
                                builder.AppendLine($"({parameter.Type})reader[{i}];");
                            }
                        }
                        builder.AppendLine("results.Add(result);");
                    }
                    builder.AppendLine("}");
                    builder.AppendLine("reader.Close();");
                    if (methodOperation.IsBatchOperation || methodOperation.SqlType == SqlType.Custom)
                    {
                        builder.AppendLine("return results;");
                    }
                    else
                    {
                        builder.AppendLine("return results.FirstOrDefault();");
                    }
                }
            }
            builder.AppendLine("}");
        }

        private void CreateSelect_Delete(MethodOperation methodOperation, IndentedStringBuilder builder, string[] whereMappings)
        {
            builder.Append($"var sql = $\"");
            if (methodOperation.IsBatchOperation)
            {
                if (methodOperation.SqlType == SqlType.Select)
                {
                    _dependencies.SQLGenerator.Select(builder, methodOperation.Table, whereMappings, true);
                }
                else
                {
                    _dependencies.SQLGenerator.Delete(builder, methodOperation.Table, whereMappings, true);
                }
            }
            else
            {
                var columnMappings = methodOperation.Table.PrimaryKey.Columns.Select(x => "@" + x.Name).ToArray();
                if (methodOperation.SqlType == SqlType.Select)
                {
                    _dependencies.SQLGenerator.Select(builder, methodOperation.Table, columnMappings, false);
                }
                else
                {
                    _dependencies.SQLGenerator.Delete(builder, methodOperation.Table, columnMappings, false);
                }
            }
            builder.AppendLine("\";");
        }

        private void CreateInsert_Update(MethodOperation methodOperation, IndentedStringBuilder builder, string requestModelName)
        {
            if (methodOperation.IsBatchOperation)
            {
                builder.AppendLine($"var sqlBuilder = new StringBuilder();");
                builder.AppendLine($"for (var i = 0; i< {requestModelName}.Count(); i++)");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    if (methodOperation.SqlType == SqlType.Insert)
                    {
                        builder.Append("sqlBuilder.AppendLine($\"");
                        var insertValues = methodOperation.InputModel.Where(x => !x.IsAutoIncrement).Select(x => "@" + x.ColumnName + "{i}");
                        _dependencies.SQLGenerator.Insert(builder, methodOperation.Table, insertValues.ToArray());
                        builder.AppendLine("\");");
                    }
                    else
                    {
                        builder.Append("sqlBuilder.AppendLine($\"");
                        var setValues = methodOperation.InputModel.Where(x => !x.IsPrimaryKey).Select(x => "@" + x.ColumnName + "{i}");
                        var whereMappings = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => "@" + x.ColumnName + "{i}");
                        _dependencies.SQLGenerator.Update(builder, methodOperation.Table, setValues.ToArray(), whereMappings.ToArray());
                        builder.AppendLine("\");");
                    }
                    if (methodOperation.SqlType == SqlType.Insert && methodOperation.InputModel.Any(x => x.IsAutoIncrement))
                    {
                        builder.Append("sqlBuilder.AppendLine($\"");
                        _dependencies.SQLGenerator.SelectLastInserted(builder, methodOperation.Table);
                        builder.AppendLine("\");");
                    }
                    else
                    {
                        builder.Append("sqlBuilder.AppendLine($\"");
                        var columnMappings = methodOperation.Table.PrimaryKey.Columns.Select(x => "@" + x.Name + "{i}").ToArray();
                        _dependencies.SQLGenerator.Select(builder, methodOperation.Table, columnMappings, false);
                        builder.AppendLine("\");");
                    }
                }
                builder.AppendLine("}");
                builder.AppendLine("var sql = sqlBuilder.ToString();");
            }
            else
            {
                builder.Append($"var sql = @\"");
                var whereMappings = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => "@" + x.ColumnName);
                if (methodOperation.SqlType == SqlType.Insert)
                {
                    var insertValues = methodOperation.InputModel.Where(x => !x.IsAutoIncrement).Select(x => "@" + x.ColumnName);
                    _dependencies.SQLGenerator.Insert(builder, methodOperation.Table, insertValues.ToArray());
                }
                else
                {
                    var setValues = methodOperation.InputModel.Where(x => !x.IsPrimaryKey).Select(x => "@" + x.ColumnName);
                    _dependencies.SQLGenerator.Update(builder, methodOperation.Table, setValues.ToArray(), whereMappings.ToArray());
                }
                if (methodOperation.InputModel.Any(x => x.IsAutoIncrement))
                {
                    _dependencies.SQLGenerator.SelectLastInserted(builder, methodOperation.Table);
                }
                else
                {
                    _dependencies.SQLGenerator.Select(builder, methodOperation.Table, whereMappings.ToArray(), false);
                }
                builder.AppendLine("\";");
            }
        }

        private void CreateCommandParameters(MethodOperation methodOperation, IndentedStringBuilder builder, string requestModelName)
        {
            builder.AppendLine("var command = connection.CreateCommand(sql, transaction, timeout);");
            if (methodOperation.IsBatchOperation)
            {
                builder.AppendLine($"for (var i = 0; i< {requestModelName}.Count(); i++)");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    foreach (var parameter in methodOperation.InputModel)
                    {
                        builder.AppendLine($"command.CreateParameter(\"@{parameter.ColumnName}\" + i, {requestModelName}[i].{parameter.PropertyName});");
                    }
                }
                builder.AppendLine("}");
            }

            foreach (var property in methodOperation.InputModel)
            {
                if (methodOperation.SqlType == SqlType.Insert && property.IsAutoIncrement)
                    continue;

                if (property.IsListType)
                {
                    builder.AppendLine($"{requestModelName}?.{property.PropertyName}.Select((x,y) => command.CreateParameter(\"@{property.ColumnName}\" + y, x)).ToArray();");
                }
                else if (property.DefaultValue != null)
                {
                    builder.AppendLine($"command.CreateParameter(\"@{property.ColumnName}\", {property.DefaultValue});");
                }
                else if (!methodOperation.IsBatchOperation)
                {
                    builder.AppendLine($"command.CreateParameter(\"@{property.ColumnName}\", {requestModelName}.{property.PropertyName});");
                }
            }
        }

        private void GenerateHelperMethods(IndentedStringBuilder builder)
        {
            /*
                    private static DbCommand CreateCommand(this DbConnection connection, string sql, DbTransaction transaction = null, int? timeout = null)
                    {
                        var dbCommand = connection.CreateCommand();
                        dbCommand.CommandText = sql;
                        dbCommand.CommandType = CommandType.Text;
                        dbCommand.Transaction = transaction;
                        dbCommand.CommandTimeout = timeout.HasValue ? timeout.Value : dbCommand.CommandTimeout;
                        return dbCommand;
                    } 
            */
            builder.AppendLine("private static DbCommand CreateCommand(this DbConnection connection, string sql, DbTransaction transaction = null, int? timeout = null)");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine("var dbCommand = connection.CreateCommand();");
                builder.AppendLine("dbCommand.CommandText = sql;");
                builder.AppendLine("dbCommand.CommandType = CommandType.Text;");
                builder.AppendLine("dbCommand.Transaction = transaction;");
                builder.AppendLine("dbCommand.CommandTimeout = timeout.HasValue ? timeout.Value : dbCommand.CommandTimeout;");
                builder.AppendLine("return dbCommand;");
            }
            builder.AppendLine("}");
            /*

                   private static void CreateParameter(this DbCommand command, string name, object value, DbType dbType)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = name;
                        parameter.Value = value;
                        parameter.DbType = dbType;
                        command.Parameters.Add(parameter);
                    }
            */
            builder.AppendLine("private static DbParameter CreateParameter(this DbCommand command, string name, object value)");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine("var parameter = command.CreateParameter();");
                builder.AppendLine("parameter.ParameterName = name;");
                builder.AppendLine("parameter.Value = value;");
                // builder.AppendLine("if (dbType != null) parameter.DbType = dbType;");
                builder.AppendLine("command.Parameters.Add(parameter);");
                builder.AppendLine("return parameter;");
            }
            builder.AppendLine("}");
        }

        private void GenerateComment(IndentedStringBuilder builder)
        {
            builder.AppendLine("//------------------------------------------------------------------------------");
            builder.AppendLine("// <auto-generated>");
            builder.AppendLine("//");
            builder.AppendLine("// Manual changes to this file may cause unexpected behavior in your application.");
            builder.AppendLine("// Manual changes to this file will be overwritten if the code is regenerated.");
            builder.AppendLine("// </auto-generated>");
            builder.AppendLine("//------------------------------------------------------------------------------");
        }
    }
}
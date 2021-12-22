using System.Collections.Generic;
using System.IO;
using BBCoders.Commons.QueryGenerator;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public class DefaultCodeGenerator : ICodeGenerator
    {
        private readonly QueryOptions _queryOptions;
        private readonly List<IOperationGenerator> _operationGenerators;

        public DefaultCodeGenerator(QueryOptions queryOptions, List<IOperationGenerator> operationGenerators)
        {
            _queryOptions = queryOptions;
            _operationGenerators = new List<IOperationGenerator>(operationGenerators);
        }

        public void Generate()
        {
            var modelBuilder = new IndentedStringBuilder();
            var serviceBuilder = new IndentedStringBuilder();
            var modelPath = "";
            var servicePath = "";
            if (_queryOptions.Language.Equals("CSharp", System.StringComparison.OrdinalIgnoreCase))
            {
                servicePath = Path.Combine(_queryOptions.OutputDirectory, _queryOptions.FileName + "." + _queryOptions.FileExtension);
                modelPath = Path.Combine(_queryOptions.OutputDirectory, (_queryOptions.ModelFileName ?? _queryOptions.FileName + "Models") + "." + _queryOptions.FileExtension);
                CreateCSharpModel(modelBuilder);
                CreateCSharpService(serviceBuilder);

            }
            else
            {
                throw new System.Exception($"Language not supported - {_queryOptions.Language}");
            }
            CreateDirectory();
            File.WriteAllText(modelPath, modelBuilder.ToString());
            File.WriteAllText(servicePath, serviceBuilder.ToString());
        }
        private void CreateDirectory()
        {
            if (!Directory.Exists(_queryOptions.OutputDirectory))
            {
                Directory.CreateDirectory(_queryOptions.OutputDirectory);
            }
            Directory.CreateDirectory(_queryOptions.OutputDirectory);
        }

        private void CreateCSharpModel(IndentedStringBuilder builder)
        {
            GenerateComment(builder);
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine();
            builder.AppendLine($"namespace {_queryOptions.PackageName}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                CreateModels(builder);
            }
            builder.AppendLine("}");
        }

        private void CreateGoModel(IndentedStringBuilder builder)
        {
            GenerateComment(builder);
            builder.AppendLine($"package {_queryOptions.PackageName}");
            builder.AppendLine();
            using (builder.Indent())
            {
                CreateModels(builder);
            }
        }

        private void CreateModels(IndentedStringBuilder builder)
        {
            foreach (var generator in _operationGenerators)
            {
                generator.GenerateModel(builder);
            }
        }


        private void CreateCSharpService(IndentedStringBuilder builder)
        {
            GenerateComment(builder);
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using System.Threading.Tasks;");
            builder.AppendLine("using System.Data;");
            builder.AppendLine("using System.Data.Common;");
            builder.AppendLine();
            builder.AppendLine($"namespace {_queryOptions.PackageName}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine($"public static class {_queryOptions.ClassName}");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    foreach (var generator in _operationGenerators)
                    {
                        generator.GenerateMethod(builder);
                    }
                    GenerateHelperMethods(builder);
                }
                builder.AppendLine("}");
            }

            builder.AppendLine("}");
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

        private void CreateGoService(IndentedStringBuilder builder)
        {
            GenerateComment(builder);
            builder.AppendLine($"package {_queryOptions.PackageName}");
            builder.AppendLine();
            builder.AppendLine("import(");
            using (builder.Indent())
            {
                builder.AppendLine("\"database/sql\"");
                builder.AppendLine("_ \"github.com/go-sql-driver/mysql\"");
                builder.AppendLine("using MySqlConnector;");
            }
            builder.AppendLine(")");
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
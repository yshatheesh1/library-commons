using System.Collections.Generic;
using System.IO;
using BBCoders.Commons.QueryConfiguration;
using BBCoders.Commons.Tools.QueryGenerator.Services;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.Tools.src.QueryGenerator.Services
{
    public class DefaultCodeGenerator : ICodeGenerator
    {
        private readonly QueryOptions _queryOptions;
        private readonly List<ISqlOperationGenerator> _operationGenerators;

        public DefaultCodeGenerator(QueryOptions queryOptions, List<ISqlOperationGenerator> operationGenerators)
        {
            _queryOptions = queryOptions;
            _operationGenerators = new List<ISqlOperationGenerator>(operationGenerators);
        }

        public void Generate()
        {
            var modelBuilder = new IndentedStringBuilder();
            var serviceBuilder = new IndentedStringBuilder();
            var modelPath = "";
            var servicePath = "";
            if (_queryOptions.Language.Equals("CSharp", System.StringComparison.OrdinalIgnoreCase))
            {
                modelPath = Path.Combine(_queryOptions.OutputDirectory, "Models.cs");
                servicePath = Path.Combine(_queryOptions.OutputDirectory, "DataServices.cs");
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
                generator.GenerateModel(_queryOptions, builder);
            }
        }


        private void CreateCSharpService(IndentedStringBuilder builder)
        {
            GenerateComment(builder);
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Data;");
            builder.AppendLine("using System.Threading.Tasks;");
            builder.AppendLine("using MySqlConnector;");
            builder.AppendLine();
            builder.AppendLine($"namespace {_queryOptions.PackageName}");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                builder.AppendLine($"public class {_queryOptions.ClassName}");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    builder.AppendLine("private readonly string _connectionString;");
                    builder.Append($"public {_queryOptions.ClassName}(string connectionString)");
                    builder.AppendLine("{ this._connectionString = connectionString; }");
                    foreach (var generator in _operationGenerators)
                    {
                        generator.GenerateMethod(_queryOptions, builder, "_connectionString");
                    }
                }
                builder.AppendLine("}");
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
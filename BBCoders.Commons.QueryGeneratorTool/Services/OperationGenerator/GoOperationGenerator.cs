using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using BBCoders.Commons.QueryGenerator;
using BBCoders.Commons.QueryGeneratorTool.Models;
using BBCoders.Commons.Utilities;
using Humanizer;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public class GoOperationGenerator : BaseOperationGenerator
    {
        public GoOperationGenerator(SqlOperationGeneratorDependencies dependencies, QueryOptions options, Language language, List<ITable> tables, List<QueryModel> sqlModels) : base(dependencies, options, language, tables, sqlModels)
        {
        }
        protected override void GenerateModel(IndentedStringBuilder builder, ClassModel classModel)
        {
            GenerateComment(builder);
            builder.AppendLine("package " + _queryOptions.ModelPackageName);
            builder.AppendLine();

            var imports = new HashSet<string>();

            AddImports(imports, classModel);
            GenerateImports(imports, builder);

            GenerateModel(builder, classModel.Name, classModel.Properties, classModel.NestedClass, true);
        }

        protected override void GenerateMethods(IndentedStringBuilder builder, List<MethodOperation> methodOperations)
        {
            GenerateComment(builder);
            // create package
            builder.AppendLine("package " + _queryOptions.PackageName);
            builder.AppendLine();
            // create imports
            var imports = new HashSet<string>();
            if (!_queryOptions.PackageName.Equals(_queryOptions.ModelPackageName))
            {
                imports.Add($"\"GoIntegrationTests/{_queryOptions.ModelPackageName}\"");
            }
            imports.Add("\"context\"");
            imports.Add("\"database/sql\"");
            imports.Add("\"strings\"");
            GenerateImports(imports, builder);

            // create type struct
            GenerateType(_queryOptions, builder);

            // create method
            GenerateMethodsBySqlType(builder, methodOperations);
        }

        private void GenerateMethodsBySqlType(IndentedStringBuilder builder, List<MethodOperation> methodOperations)
        {
            foreach (var methodOperation in methodOperations)
            {
                if (methodOperation.SqlType == SqlType.Select || methodOperation.SqlType == SqlType.Delete)
                {
                    if (methodOperation.IsBatchOperation)
                    {
                        GenerateMethod(builder, methodOperation, this.GenerateSelectorDeleteBatchMethodImpl);
                    }
                    else
                    {
                        GenerateMethod(builder, methodOperation, this.GenerateSelectorDeleteMethodImpl);
                    }
                }
                if (methodOperation.SqlType == SqlType.Insert)
                {
                    if (methodOperation.IsBatchOperation)
                    {
                        GenerateMethod(builder, methodOperation, this.GenerateInsertBatchMethodImpl);
                    }
                    else
                    {
                        GenerateMethod(builder, methodOperation, this.GenerateInsertMethodImpl);
                    }
                }
                if (methodOperation.SqlType == SqlType.Update)
                {
                    if (methodOperation.IsBatchOperation)
                    {
                        GenerateMethod(builder, methodOperation, this.GenerateUpdateBatchMethodImpl);
                    }
                    else
                    {
                        GenerateMethod(builder, methodOperation, this.GenerateUpdateMethodImpl);
                    }
                }
            }

        }

        private void GenerateSelectorDeleteMethodImpl(IndentedStringBuilder builder, MethodOperation methodOperation, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {
            var placeholders = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => "?").ToArray();
            var args = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => $"{requestModelRef}.{x.PropertyName}").ToArray();
            var responseArgs = methodOperation.InputModel.Select(x => $"&{responseModelRef}.{x.PropertyName}").ToArray();

            builder.Append("sqlString := \"");
            if (methodOperation.SqlType == SqlType.Select)
            {
                _dependencies.SQLGenerator.Select(builder, methodOperation.Table, placeholders.ToArray());
            }
            else
            {
                _dependencies.SQLGenerator.Delete(builder, methodOperation.Table, placeholders.ToArray());
            }
            builder.AppendLine("\"");

            if (methodOperation.SqlType == SqlType.Select)
            {
                GenerateQueryRow(builder, args, responseArgs, requestModelRef, requestModelName, responseModelRef, responseModelName, classModelRef, className);
            }
            else
            {
                GenerateExec(builder, args, requestModelRef, requestModelName, responseModelRef, responseModelName, classModelRef, className);

            }
        }


        private void GenerateInsertMethodImpl(IndentedStringBuilder builder, MethodOperation methodOperation, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {
            var insertPlaceholders = methodOperation.InputModel.Where(x => !x.IsAutoIncrement).Select(x => "?").ToList();
            var insertArgs = methodOperation.InputModel.Where(x => !x.IsAutoIncrement).Select(x => $"{requestModelRef}.{x.PropertyName}").ToList();
            var responseArgs = methodOperation.InputModel.Select(x => $"&{responseModelRef}.{x.PropertyName}").ToList();

            builder.Append("sqlString := \"");
            _dependencies.SQLGenerator.Insert(builder, methodOperation.Table, insertPlaceholders.ToArray());
            if (methodOperation.InputModel.Any(x => x.IsAutoIncrement))
            {
                _dependencies.SQLGenerator.SelectLastInserted(builder, methodOperation.Table);
            }
            else
            {
                var wherePlaceholders = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => "?");
                insertPlaceholders.AddRange(wherePlaceholders);
                var whereArgs = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => $"{requestModelRef}.{x.PropertyName}");
                insertArgs.AddRange(whereArgs);
                _dependencies.SQLGenerator.Select(builder, methodOperation.Table, wherePlaceholders.ToArray(), false);
            }
            builder.AppendLine("\"");

            GenerateQueryRow(builder, insertArgs.ToArray(), responseArgs.ToArray(), requestModelRef, requestModelName, responseModelRef, responseModelName, classModelRef, className);
        }


        private void GenerateUpdateMethodImpl(IndentedStringBuilder builder, MethodOperation methodOperation, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {
            var updateSetPlaceholders = methodOperation.InputModel.Where(x => !x.IsPrimaryKey).Select(x => "?").ToList();
            var updateWherePlaceholders = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => "?").ToList();
            var updateSetArgs = methodOperation.InputModel.Where(x => !x.IsPrimaryKey).Select(x => $"{requestModelRef}.{x.PropertyName}").ToList();
            var updateWhereArgs = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => $"{requestModelRef}.{x.PropertyName}").ToList();
            var responseArgs = methodOperation.InputModel.Select(x => $"&{responseModelRef}.{x.PropertyName}").ToArray();

            builder.Append("sqlString := \"");
            _dependencies.SQLGenerator.Update(builder, methodOperation.Table, updateSetPlaceholders.ToArray(), updateWherePlaceholders.ToArray());
            var selectWherePlaceholders = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => "?");
            var selectWhereArgs = methodOperation.InputModel.Where(x => x.IsPrimaryKey).Select(x => $"{requestModelRef}.{x.PropertyName}");
            _dependencies.SQLGenerator.Select(builder, methodOperation.Table, selectWherePlaceholders.ToArray(), false);

            builder.AppendLine("\"");

            var placeholders = updateSetPlaceholders.Concat(updateWherePlaceholders).Concat(selectWherePlaceholders).ToArray();
            var args = updateSetArgs.Concat(updateWhereArgs).Concat(selectWhereArgs).ToArray();
            GenerateQueryRow(builder, args, responseArgs, requestModelRef, requestModelName, responseModelRef, responseModelName, classModelRef, className);
        }

        private void GenerateSelectorDeleteBatchMethodImpl(IndentedStringBuilder builder, MethodOperation methodOperation, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {
            var responseArgs = methodOperation.InputModel.Select(x => $"&{responseModelRef}.{x.PropertyName}").ToArray();
            foreach (var model in methodOperation.InputModel.Where(x => x.IsPrimaryKey))
            {
                var placeholder = $"{model.PropertyName.Camelize()}Placeholders";
                var arg = $"{model.PropertyName.Camelize()}Args";
                builder.AppendLine($"var {placeholder} []string");
                builder.AppendLine($"var {arg} []interface" + "{}");
            }
            var alias = requestModelRef.Substring(0, 1);
            builder.AppendLine($"for _, {alias} := range {requestModelRef} " + "{");
            using (builder.Indent())
            {
                foreach (var model in methodOperation.InputModel.Where(x => x.IsPrimaryKey))
                {
                    var placeholder = $"{model.PropertyName.Camelize()}Placeholders";
                    var arg = $"{model.PropertyName.Camelize()}Args";
                    builder.AppendLine($"{placeholder} = append({placeholder}, \"?\")");
                    builder.AppendLine($"{arg} = append({arg}, {alias}.{model.PropertyName})");
                }
            }
            builder.AppendLine("}");

            var whereMappings = new List<string>();
            var args = new List<string>();
            foreach (var model in methodOperation.InputModel.Where(x => x.IsPrimaryKey))
            {
                var placeholder = $"{model.PropertyName.Camelize()}Placeholders";
                var arg = $"{model.PropertyName.Camelize()}Args";
                var mapping = $"strings.Join({placeholder}, \", \")";
                whereMappings.Add(mapping);
                args.Add(arg);
            }
            builder.Append("sqlString := \"");
            if (methodOperation.SqlType == SqlType.Select)
            {
                _dependencies.SQLGenerator.Select(builder, methodOperation.Table, whereMappings.ToArray(), true);
            }
            else
            {
                _dependencies.SQLGenerator.Delete(builder, methodOperation.Table, whereMappings.ToArray(), true);
            }
            builder.AppendLine("\"");

            if (methodOperation.SqlType == SqlType.Select)
            {
                GenerateQueryRows(builder, args.ToArray(), responseArgs, requestModelRef, requestModelName, responseModelRef, responseModelName, classModelRef, className);
            }
            else
            {
                GenerateExec(builder, args.ToArray(), requestModelRef, requestModelName, responseModelRef, responseModelName, classModelRef, className);
            }
        }

        private void GenerateInsertBatchMethodImpl(IndentedStringBuilder builder, MethodOperation methodOperation, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {

        }

        private void GenerateUpdateBatchMethodImpl(IndentedStringBuilder builder, MethodOperation methodOperation, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {

        }


        private void GenerateCustomMethodImpl(IndentedStringBuilder builder, MethodOperation methodOperation, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {

        }

        private void GenerateMethod(IndentedStringBuilder builder, MethodOperation methodOperation, Action<IndentedStringBuilder, MethodOperation, string, string, string, string, string, string> methodImplAction)
        {
            Func<bool> pluralizeAction = () => methodOperation.IsBatchOperation || methodOperation.SqlType == SqlType.Custom;
            var methodName = methodOperation.MethodName;
            var className = _queryOptions.ClassName;
            var classReference = _queryOptions.ClassName.Camelize();
            var requestModel = _queryOptions.ModelPackageName + "." + methodOperation.InputModelName;
            var requestModelRef = methodOperation.IsBatchOperation ? methodOperation.InputModelName.Camelize().Pluralize() : methodOperation.InputModelName.Camelize();
            var responseModel = _queryOptions.ModelPackageName + "." + methodOperation.OutputModelName;
            var responseModelRef = pluralizeAction() ? methodOperation.OutputModelName.Camelize().Pluralize() : methodOperation.OutputModelName.Camelize();
            var inputType = methodOperation.IsBatchOperation ? $"{requestModelRef} ...{requestModel}" : $"{requestModelRef} {requestModel}";
            var returnType = methodOperation.HasResult ? (methodOperation.IsBatchOperation || methodOperation.SqlType == SqlType.Custom) ? $"([]{responseModel}, error)" : $"({responseModel}, error)" : "int64";

            builder.AppendLine($"func ({classReference} {className}) {methodName}(ctx context.Context, {inputType}) {returnType} " + "{");
            using (builder.Indent())
            {
                var multiple = methodOperation.IsBatchOperation ? "..." : "";
                builder.AppendLine($"return {_queryOptions.ClassName.Camelize()}.{methodName}Transaction(ctx, nil, {requestModelRef}{multiple})");
            }
            builder.AppendLine("}").AppendLine();
            // method with transaction
            builder.AppendLine($"func ({classReference} {className}) {methodName}Transaction(ctx context.Context, tx *sql.Tx, {inputType}) {returnType} " + "{");
            using (builder.Indent())
            {
                methodImplAction(builder, methodOperation, requestModelRef, requestModel, responseModelRef, responseModel, classReference, className);
            }
            builder.AppendLine("}").AppendLine();
        }

        private void GenerateQueryRows(IndentedStringBuilder builder, string[] args, string[] responseArgs, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {
            builder.AppendLine();
            builder.AppendLine("var rows *sql.Rows");
            builder.AppendLine("var err error");
            builder.AppendLine("if tx != nil {");
            using (builder.Indent())
            {
                builder.AppendLine($"rows, err = tx.QueryContext(ctx, sqlString, {string.Join(", ", args)})");
            }
            builder.AppendLine("} else {");
            using (builder.Indent())
            {
                builder.AppendLine($"rows, err = {classModelRef}.conn.QueryContext(ctx, sqlString, {string.Join(", ", args)})");
            }
            builder.AppendLine("}");
            WriteError(builder, "nil", "err");
            builder.AppendLine("defer rows.Close()");
            builder.AppendLine();
            builder.AppendLine($"{responseModelRef} := []{responseModelName}" + "{}");
            builder.AppendLine("for rows.Next() || (rows.NextResultSet() && rows.Next()) " + "{");
            using (builder.Indent())
            {
                builder.AppendLine($"err := row.Scan({string.Join(", ", responseArgs)})");
                WriteError(builder, "nil", "err");
            }
            builder.AppendLine("}");
            builder.AppendLine($"return {responseModelRef}, nil");
        }

        private void GenerateQueryRow(IndentedStringBuilder builder, string[] args, string[] responseArgs, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {
            builder.AppendLine("var row *sql.Row");
            builder.AppendLine("if tx != nil {");
            using (builder.Indent())
            {
                builder.AppendLine($"row = tx.QueryRowContext(ctx, sqlString, {string.Join(", ", args)})");
            }
            builder.AppendLine("} else {");
            using (builder.Indent())
            {
                builder.AppendLine($"row = {classModelRef}.conn.QueryRowContext(ctx, sqlString, {string.Join(", ", args)})");
            }
            builder.AppendLine("}");
            builder.AppendLine($"var {responseModelRef} *{responseModelName}");
            builder.AppendLine($"err := row.Scan({string.Join(", ", responseArgs)})");
            WriteError(builder, responseModelRef, "err");
            builder.AppendLine($"return {responseModelRef}, nil");
        }

        private void GenerateExec(IndentedStringBuilder builder, string[] args, string requestModelRef, string requestModelName, string responseModelRef, string responseModelName, string classModelRef, string className)
        {
            builder.AppendLine("var result sql.Result");
            builder.AppendLine("var err error");
            builder.AppendLine("if tx != nil {");
            using (builder.Indent())
            {
                builder.AppendLine($"result, err = tx.ExecContext(ctx, sqlString, {string.Join(", ", args)})");
            }
            builder.AppendLine("} else {");
            using (builder.Indent())
            {
                builder.AppendLine($"result, err = {classModelRef}.conn.ExecContext(ctx, sqlString, {string.Join(", ", args)})");
            }
            builder.AppendLine("}");
            WriteError(builder, "0", "err");
            builder.AppendLine($"return result.RowsAffected()");
        }

        private void WriteError(IndentedStringBuilder builder, string returnValue, string returnErr)
        {
            builder.AppendLine($"if err != nil " + "{");
            using (builder.Indent())
            {
                builder.AppendLine($"return {returnValue}, {returnErr}");
            }
            builder.AppendLine("}");
        }


        private void AddImports(HashSet<string> imports, ClassModel classModel)
        {
            foreach (var property in classModel.Properties)
            {
                var typeName = _language.Type[property.CSharpType];
                if (typeName.Equals("time.Time"))
                {
                    imports.Add("\"time\"");
                }
            }
            foreach (var nestedModel in classModel.NestedClass)
            {
                AddImports(imports, nestedModel);
            }
        }

        private void GenerateType(QueryOptions queryOptions, IndentedStringBuilder builder)
        {
            // type ScheduleSiteRepo struct {
            // 	conn *sql.DB
            // }
            builder.AppendLine($"type {queryOptions.ClassName} struct " + "{");
            using (builder.Indent())
            {
                builder.AppendLine("conn *sql.DB");
            }
            builder.AppendLine("}").AppendLine();

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
            builder.AppendLine();
        }

        private void GenerateImports(HashSet<string> imports, IndentedStringBuilder builder)
        {
            /*
            import (
                "context"
                "database/sql"
                "strings"
            )
            */
            if (imports.Count == 0)
                return;
            builder.AppendLine("import (");
            using (builder.Indent())
            {
                imports.Select(x => builder.AppendLine(x)).ToArray();
            }
            builder.AppendLine(")");
            builder.AppendLine();
        }

        private void GenerateModel(IndentedStringBuilder builder, string className, List<PropertyModel> properties, List<ClassModel> nestedClasses, bool isparent)
        {
            if (isparent)
            {
                builder.Append($"type ");
            }
            builder.Append($"{className} struct ");
            builder.AppendLine("{");
            using (builder.Indent())
            {
                foreach (var property in properties)
                {
                    var typeName = _language.Type[property.CSharpType];
                    var listType = property.IsList ? "[]" : "";
                    builder.AppendLine($"{property.Name} {listType}{typeName}");
                }
                foreach (var nestedModel in nestedClasses)
                {
                    GenerateModel(builder, nestedModel.Name, nestedModel.Properties, nestedModel.NestedClass, false);
                }
            }
            builder.AppendLine("}");
        }

    }
}
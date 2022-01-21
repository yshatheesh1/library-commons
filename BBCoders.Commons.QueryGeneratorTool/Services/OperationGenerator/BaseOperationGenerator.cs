using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BBCoders.Commons.QueryGenerator;
using BBCoders.Commons.QueryGeneratorTool.Helpers;
using BBCoders.Commons.QueryGeneratorTool.Models;
using BBCoders.Commons.Utilities;
using Humanizer;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public abstract class BaseOperationGenerator : IOperationGenerator
    {
        protected ISqlGenerationHelper _sqlGenerationHelper;
        protected IRelationalTypeMappingSource _relationalTypeMappingSource;
        protected SqlOperationGeneratorDependencies _dependencies;
        protected QueryOptions _queryOptions;
        protected List<ITable> _tables;
        protected List<QueryModel> _sqlModels;
        protected Language _language;

        public BaseOperationGenerator(SqlOperationGeneratorDependencies dependencies, QueryOptions options, Language language, List<ITable> tables, List<QueryModel> sqlModels)
        {
            _dependencies = dependencies;
            _relationalTypeMappingSource = dependencies.relationalTypeMappingSource;
            _queryOptions = options;
            _tables = tables;
            _sqlModels = sqlModels;
            _language = language;
        }

        protected abstract void GenerateMethods(IndentedStringBuilder builder, List<MethodOperation> methodOperation);

        protected abstract void GenerateModel(IndentedStringBuilder builder, ClassModel classModel);

        public void Generate()
        {
            if (!Directory.Exists(_queryOptions.OutputDirectory))
            {
                Reporter.WriteVerbose("Creating directory for services - " + _queryOptions.OutputDirectory);
                Directory.CreateDirectory(_queryOptions.OutputDirectory);
            }
            if (!Directory.Exists(_queryOptions.ModelOutputDirectory))
            {
                Reporter.WriteVerbose("Creating directory for models - " + _queryOptions.ModelOutputDirectory);
                Directory.CreateDirectory(_queryOptions.ModelOutputDirectory);
            }
            GenerateModels();
            GenerateMethods();
        }

        protected void GenerateModels()
        {
            // default models
            foreach (var table in _tables)
            {
                // key selector 
                GenerateModel(new ClassModel()
                {
                    Name = GetInputModelName(table),
                    Properties = table.PrimaryKey.Columns.Select(x => GetPropertyModel(x)).ToList()
                });
                // output model 
                GenerateModel(new ClassModel()
                {
                    Name = GetOutputModelName(table),
                    Properties = table.Columns.Select(x => GetPropertyModel(x)).ToList()
                });
            }
            // user defined models
            foreach (var model in _sqlModels)
            {
                GenerateModel(new ClassModel()
                {
                    Name = GetCustomInputModelName(model.MethodName),
                    Properties = model.Parameters.Select(x => new PropertyModel() { Name = x.Name, Type = x.Type, IsList = x.IsList }).ToList()
                });

                // generate output model
                var responseModel = new ClassModel()
                {
                    Name = GetCustomOutputModelName(model.MethodName),
                    // class level properties
                    Properties = model.Projections.Where(x => x.Table == null).Select(x => GetPropertyModel(x)).ToList()
                };
                // nested class level properties
                var projectionModels = model.Projections.Where(x => x.Table != null).GroupBy(x => x.Table).ToDictionary(x => x.Key, y => y.ToList());
                foreach (var projectionModel in projectionModels)
                {
                    var className = projectionModel.Key.Singularize().Pascalize();
                    responseModel.NestedClass.Add(new ClassModel()
                    {
                        Name = className,
                        Properties = projectionModel.Value.Select(x => GetPropertyModel(x)).ToList()
                    });
                }
                GenerateModel(responseModel);
            }
        }
        protected void GenerateMethods()
        {
            var serviceBuilder = new IndentedStringBuilder();
            var methods = new List<MethodOperation>();
            // add default methods
            foreach (var table in _tables)
            {
                methods.Add(GetSelectMethodOperation(table, "SelectBatch", true));
                methods.Add(GetInsertMethodOperation(table, "InsertBatch", true));
                methods.Add(GetUpdateMethodOperation(table, "UpdateBatch", true));
                methods.Add(GetDeleteMethodOperation(table, "DeleteBatch", true));

                methods.Add(GetSelectMethodOperation(table, "Select", false));
                methods.Add(GetInsertMethodOperation(table, "Insert", false));
                methods.Add(GetUpdateMethodOperation(table, "Update", false));
                methods.Add(GetDeleteMethodOperation(table, "Delete", false));
            }
            // add custom methods
            foreach (var model in _sqlModels)
            {
                methods.Add(GenerateCustomMethod(model));
            }
            GenerateMethods(serviceBuilder, methods);
            var servicePath = Path.Combine(_queryOptions.OutputDirectory, _queryOptions.FileName + "." + _language.FileExtension);
            File.WriteAllText(servicePath, serviceBuilder.ToString());
        }


        private MethodOperation GetSelectMethodOperation(ITable table, string MethodPrefix, bool isBatch)
        {
            var outputModel = table.Columns.Select(x => x.CreateModelParameter(table, _language, _relationalTypeMappingSource)).ToList();
            var primaryKeyProperties = table.PrimaryKey.Columns.Select(x => x.CreateModelParameter(table, _language, _relationalTypeMappingSource)).ToList();
            var methodOp = new MethodOperation()
            {
                MethodName = MethodPrefix + GetEntityName(table).Pascalize(),
                InputModelName = GetInputModelName(table),
                InputModel = primaryKeyProperties,
                SqlType = SqlType.Select,
                HasResult = true,
                OutputModelName = GetOutputModelName(table),
                OutputModel = outputModel,
                IsBatchOperation = isBatch,
                Table = outputModel
            };
            Reporter.WriteVerbose("Generating select method operation - " + JsonSerializer.Serialize(methodOp, new JsonSerializerOptions() { WriteIndented = true }));
            return methodOp;
        }
        protected MethodOperation GetInsertMethodOperation(ITable table, string MethodPrefix, bool isBatch)
        {
            var outputModel = table.Columns.Select(x => x.CreateModelParameter(table, _language, _relationalTypeMappingSource)).ToList();
            var methodOp = new MethodOperation()
            {
                MethodName = MethodPrefix + GetEntityName(table).Pascalize(),
                InputModelName = GetOutputModelName(table),
                InputModel = outputModel,
                SqlType = SqlType.Insert,
                HasResult = true,
                OutputModelName = GetOutputModelName(table),
                OutputModel = outputModel,
                IsBatchOperation = isBatch,
                Table = outputModel
            };
            Reporter.WriteVerbose("Generating insert method operation - " + JsonSerializer.Serialize(methodOp, new JsonSerializerOptions() { WriteIndented = true }));
            return methodOp;
        }

        protected MethodOperation GetUpdateMethodOperation(ITable table, string MethodPrefix, bool isBatch)
        {
            var properties = table.Columns.Select(x => x.CreateModelParameter(table, _language, _relationalTypeMappingSource)).ToList();
            var methodOp = new MethodOperation()
            {
                MethodName = MethodPrefix + GetEntityName(table).Pascalize(),
                InputModelName = GetOutputModelName(table),
                InputModel = properties,
                SqlType = SqlType.Update,
                HasResult = true,
                OutputModelName = GetOutputModelName(table),
                OutputModel = properties,
                IsBatchOperation = isBatch,
                Table = properties
            };
            Reporter.WriteVerbose("Generating update method operation - " + JsonSerializer.Serialize(methodOp, new JsonSerializerOptions() { WriteIndented = true }));
            return methodOp;
        }

        protected MethodOperation GetDeleteMethodOperation(ITable table, string MethodPrefix, bool isBatch)
        {
            var primaryKeyProperties = table.PrimaryKey.Columns.Select(x => x.CreateModelParameter(table, _language, _relationalTypeMappingSource)).ToList();
            var methodOp = new MethodOperation()
            {
                MethodName = MethodPrefix + GetEntityName(table).Pascalize(),
                InputModelName = GetInputModelName(table),
                InputModel = primaryKeyProperties,
                SqlType = SqlType.Delete,
                HasResult = false,
                OutputModelName = GetOutputModelName(table),
                IsBatchOperation = isBatch,
                Table = table.GetMappings(_language, _relationalTypeMappingSource)
            };
            Reporter.WriteVerbose("Generating delete method operation - " + JsonSerializer.Serialize(methodOp, new JsonSerializerOptions() { WriteIndented = true }));
            return methodOp;
        }

        private MethodOperation GenerateCustomMethod(QueryModel sqlModel)
        {
            Reporter.WriteVerbose("Generating custom method - " + sqlModel.MethodName);
            var inputPrameters1 = sqlModel.Bindings.Select(x => CreateModelParameter(x));
            // generate output model
            var outputParameter = sqlModel.Projections.Where(x => x.Table == null).Select(x => new ModelParameter()
            {
                ColumnName = x.Name,
                PropertyName = x.Name,
                IsNullable = x.IsNullable,
                Type = x.Type,
                IsvalueType = x.IsValueType

            }).ToList();
            // nested class level properties
            var projectionModels = sqlModel.Projections.Where(x => x.Table != null).GroupBy(x => x.Table).ToDictionary(x => x.Key, y => y.ToList());
            foreach (var projectionModel in projectionModels)
            {
                var className = projectionModel.Key.Singularize().Pascalize();
                var nestedOutputParameters = projectionModel.Value.Select(x => new ModelParameter()
                {
                    ColumnName = className + "." + x.Name,
                    PropertyName = className + "." + x.Name,
                    IsNullable = x.IsNullable,
                    IsvalueType = x.IsValueType,
                    Type = x.Type
                }).ToList();
                outputParameter.AddRange(nestedOutputParameters);
            }

            var methodOp = new MethodOperation()
            {
                MethodName = sqlModel.MethodName.Pascalize(),
                InputModelName = GetCustomInputModelName(sqlModel.MethodName),
                InputModel = inputPrameters1.ToList(),
                SqlType = SqlType.Custom,
                CustomSql = sqlModel.Sql,
                HasResult = true,
                OutputModelName = GetCustomOutputModelName(sqlModel.MethodName),
                OutputModel = outputParameter,
                IsBatchOperation = false,
            };
            Reporter.WriteVerbose("Generating custom method operation - " + JsonSerializer.Serialize(methodOp, new JsonSerializerOptions() { WriteIndented = true }));
            return methodOp;
        }

        private void GenerateModel(ClassModel classModel)
        {
            var builder = new IndentedStringBuilder();
            string jsonString = JsonSerializer.Serialize(classModel, new JsonSerializerOptions { WriteIndented = true });
            Reporter.WriteVerbose("Creating Model - " + jsonString);
            GenerateModel(builder, classModel);
            var modelPath = Path.Combine(_queryOptions.ModelOutputDirectory, classModel.Name + "." + _language.FileExtension);
            File.WriteAllText(modelPath, builder.ToString());
        }

        private PropertyModel GetPropertyModel(IColumn column)
        {
            var property = column.PropertyMappings.First().Property;
            return new PropertyModel()
            {
                Name = property.Name,
                IsNullable = property.IsNullable,
                IsValueType = property.ClrType.IsValueType,
                Type = property.GetDbTypeName(_language, _relationalTypeMappingSource)
            };
        }

        private PropertyModel GetPropertyModel(Projection sqlProjection)
        {
            return new PropertyModel()
            {
                Name = sqlProjection.Name,
                IsNullable = sqlProjection.IsNullable,
                IsValueType = sqlProjection.IsValueType,
                Type = sqlProjection.Type
            };
        }

        private ModelParameter CreateModelParameter(Binding sqlBinding)
        {
            return new ModelParameter()
            {
                ColumnName = sqlBinding.Name,
                PropertyName = sqlBinding.Value?.Name,
                DefaultValue = sqlBinding.DefaultValue,
                Type =  sqlBinding.Value?.Type,
                IsListType = sqlBinding.Value != null ? sqlBinding.Value.IsList : false
            };
        }

        protected String GetEntityName(ITable table) => table.EntityTypeMappings.First().EntityType.ClrType.Name;
        protected String GetInputModelName(ITable table) => GetEntityName(table).Pascalize().Singularize() + "Key";
        protected String GetOutputModelName(ITable table) => GetEntityName(table).Pascalize().Singularize() + "Model";
        protected String GetCustomInputModelName(string method) => method.Pascalize() + "RequestModel";
        protected String GetCustomOutputModelName(string method) => method.Pascalize() + "ResponseModel";
    }
}
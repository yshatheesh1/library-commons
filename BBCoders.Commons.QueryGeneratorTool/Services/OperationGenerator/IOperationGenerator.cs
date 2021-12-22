using BBCoders.Commons.QueryGenerator;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.QueryGeneratorTool.Services
{
    public interface IOperationGenerator
    {
        void GenerateSql(IndentedStringBuilder builder);
        void GenerateModel(IndentedStringBuilder builder);
        void GenerateMethod(IndentedStringBuilder builder);
    }
}
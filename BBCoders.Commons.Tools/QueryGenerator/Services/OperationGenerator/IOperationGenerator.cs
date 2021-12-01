using BBCoders.Commons.QueryConfiguration;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public interface IOperationGenerator
    {
        void GenerateSql(IndentedStringBuilder builder);
        void GenerateModel(IndentedStringBuilder builder);
        void GenerateMethod(IndentedStringBuilder builder, string connectionString);
    }
}
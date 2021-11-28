using BBCoders.Commons.QueryConfiguration;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public interface ISqlOperationGenerator
    {
        void GenerateSql(IndentedStringBuilder migrationCommandListBuilder);
        void GenerateModel(IndentedStringBuilder builder);
        void GenerateMethod(IndentedStringBuilder builder, string connectionString);
    }
}
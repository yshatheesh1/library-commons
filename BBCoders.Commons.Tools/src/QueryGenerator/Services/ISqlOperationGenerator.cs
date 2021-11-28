using BBCoders.Commons.QueryConfiguration;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BBCoders.Commons.Tools.QueryGenerator.Services
{
    public interface ISqlOperationGenerator
    {
        void GenerateSql(IndentedStringBuilder migrationCommandListBuilder);
        void GenerateModel(QueryOptions queryOptions, IndentedStringBuilder builder);
        void GenerateMethod(QueryOptions queryOptions,  IndentedStringBuilder builder, string connectionString);
    }
}
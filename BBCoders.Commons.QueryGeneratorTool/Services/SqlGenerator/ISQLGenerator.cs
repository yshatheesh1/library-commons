using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services.SqlGenerator
{
    public interface ISQLGenerator
    {
        void SelectLastInserted(IndentedStringBuilder builder, ITable table);
        void Select(IndentedStringBuilder builder, ITable table, string[] whereMappings, bool isDynamic = false);
        void Delete(IndentedStringBuilder builder, ITable table, string[] whereMappings, bool isDynamic = false);
        void Insert(IndentedStringBuilder builder, ITable table, string[] insertValues);
        void Update(IndentedStringBuilder builder, ITable table, string[] setMappings, string[] whereMappings);
    }
}
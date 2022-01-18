using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBCoders.Commons.QueryGeneratorTool.Services.SqlGenerator
{
    public interface ISQLGenerator
    { 
        void SelectBatch(IndentedStringBuilder builder, ITable table, string[] whereMappings);
        void InsertBatch(IndentedStringBuilder builder, ITable table, string[] whereMappings, string placeholder);
        void DeleteBatch(IndentedStringBuilder builder, ITable table, string[] whereMappings);
        void UpdateBatch(IndentedStringBuilder builder, ITable table, string[] whereMappings, string placeholder);

        void Select(IndentedStringBuilder builder, ITable table, string placeholder = "");
        void Insert(IndentedStringBuilder builder, ITable table);
        void Delete(IndentedStringBuilder builder, ITable table);
        void Update(IndentedStringBuilder builder, ITable table);
    }
}
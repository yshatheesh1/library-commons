using Microsoft.EntityFrameworkCore.Infrastructure;
using BBCoders.Commons.QueryGeneratorTool.Models;
using System.Collections.Generic;

namespace BBCoders.Commons.QueryGeneratorTool.Services.SqlGenerator
{
    public interface ISQLGenerator
    {
        void SelectLastInserted(IndentedStringBuilder builder, List<ModelParameter> table);
        void Select(IndentedStringBuilder builder, List<ModelParameter> table, string[] whereMappings, bool isDynamic = false);
        void Delete(IndentedStringBuilder builder, List<ModelParameter> table, string[] whereMappings, bool isDynamic = false);
        void Insert(IndentedStringBuilder builder, List<ModelParameter> table, string[] insertValues);
        void Update(IndentedStringBuilder builder, List<ModelParameter> table, string[] setMappings, string[] whereMappings);
    }
}
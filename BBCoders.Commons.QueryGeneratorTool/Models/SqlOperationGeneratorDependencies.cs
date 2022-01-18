using BBCoders.Commons.QueryGeneratorTool.Services.SqlGenerator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.QueryGeneratorTool.Models
{
    public class SqlOperationGeneratorDependencies
    {
        public SqlOperationGeneratorDependencies(ISQLGenerator SQLGenerator, IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            this.SQLGenerator = SQLGenerator;
            this.relationalTypeMappingSource = relationalTypeMappingSource;

        }
        public ISQLGenerator SQLGenerator { get; }
        public IRelationalTypeMappingSource relationalTypeMappingSource { get; }
    }
}
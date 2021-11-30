using Microsoft.EntityFrameworkCore.Storage;

namespace BBCoders.Commons.Tools.QueryGenerator.Models
{
    public class SqlOperationGeneratorDependencies
    {
        public SqlOperationGeneratorDependencies(ISqlGenerationHelper sqlGenerationHelper, IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            this.sqlGenerationHelper = sqlGenerationHelper;
            this.relationalTypeMappingSource = relationalTypeMappingSource;

        }
        public ISqlGenerationHelper sqlGenerationHelper { get; }

        public IRelationalTypeMappingSource relationalTypeMappingSource { get; }
    }
}
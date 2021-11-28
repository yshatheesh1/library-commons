
using System;
using System.IO;
using BBCoders.Commons.QueryConfiguration; 
using BBCoders.Commons.Tools.IntegrationTests.Context;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class QueryConfiguration : IQueryConfiguration<TestContext>
    {

        public void CreateQuery(TestContext context, QueryOperations queryOperations)
        {
            queryOperations.Add<ScheduleSite>();
        }

        public QueryOptions GetQueryOptions()
        {
            return new QueryOptions()
            {
                ClassName = "ScheduleSiteRepository",
                Language = "csharp",
                OutputDirectory = Path.Combine(QueryGeneratorTests.CurrentDirectory, QueryGeneratorTests.TestDirectory),
                PackageName = "BBCoders.Example.DataServices",
                FileExtension = "cs",
                FileName = "ScheduleSiteRepository",
                ModelFileName = "ScheduleSiteDataModel"
            };
        }
    }
}
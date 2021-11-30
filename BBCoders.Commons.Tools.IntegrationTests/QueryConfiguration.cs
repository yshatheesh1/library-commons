using System;
using System.IO;
using System.Linq;
using BBCoders.Commons.QueryConfiguration;
using BBCoders.Commons.Tools.IntegrationTests.Context;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class QueryConfigurationTest1 : IQueryConfiguration<TestContext>
    {
        public void CreateQuery(TestContext context, QueryOperations queryOperations)
        {
            queryOperations.Add<ScheduleSite>();
            queryOperations.Add<Guid>("GetSheduleSiteStatus", (id) => context.ScheduleSites.Where(x => x.ScheduleSiteId == id));
        }

        public QueryOptions GetQueryOptions()
        {
            return new QueryOptions()
            {
                ClassName = "ScheduleSiteRepository",
                Language = "csharp",
                OutputDirectory = Path.Combine(QueryGeneratorTests.TestCurrentDirectory, "TestServices"),
                PackageName = "BBCoders.Example.DataServices",
                FileExtension = "cs",
                FileName = "ScheduleSiteRepository",
                ModelFileName = "ScheduleSiteDataModel"
            };
        }
    }
    public class QueryConfigurationTest2 : IQueryConfiguration<TestContext>
    {
        public void CreateQuery(TestContext context, QueryOperations queryOperations)
        {
            queryOperations.Add<Fingerprint>();
        }

        public QueryOptions GetQueryOptions()
        {
            return new QueryOptions()
            {
                ClassName = "FingerprintRepository",
                Language = "csharp",
                OutputDirectory = Path.Combine(QueryGeneratorTests.TestCurrentDirectory, "TestServices"),
                PackageName = "BBCoders.Example.DataServices",
                FileExtension = "cs",
                FileName = "FingerprintRepository",
                ModelFileName = "FingerprintDataModel"
            };
        }
    }

     public class QueryConfigurationTest3 : IQueryConfiguration<TestContext>
    {
        public void CreateQuery(TestContext context, QueryOperations queryOperations)
        {
            queryOperations.Add<State>();
        }

        public QueryOptions GetQueryOptions()
        {
            return new QueryOptions()
            {
                ClassName = "StateRepository",
                Language = "csharp",
                OutputDirectory = Path.Combine(QueryGeneratorTests.TestCurrentDirectory, "TestServices"),
                PackageName = "BBCoders.Example.DataServices",
                FileExtension = "cs",
                FileName = "StateRepository",
                ModelFileName = "StateDataModel"
            };
        }
    }
}
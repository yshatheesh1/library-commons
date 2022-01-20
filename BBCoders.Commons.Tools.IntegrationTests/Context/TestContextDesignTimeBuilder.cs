using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql;

namespace BBCoders.Commons.Tools.IntegrationTests.Context
{
    public class TestContextDesignTimeBuilder : IDesignTimeDbContextFactory<TestContext>
    {
        public static string ConnectionString = "Server=localhost;port=3306;database=usermanagement;uid=usermanagement_test;pwd=usermanagement_test;";
        public TestContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TestContext>().UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));
            var testContext = new TestContext(builder.Options);
            return testContext;
        }
    }
    public class MyDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkMySql();
        }
    }
}
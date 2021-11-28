using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;

namespace BBCoders.Commons.Tools.IntegrationTests.Context
{
    public class TestContextDesignTimeBuilder : IDesignTimeDbContextFactory<TestContext>
    {
        public TestContext CreateDbContext(string[] args)
        {
            var connectionString = "Server=localhost;port=3306;database=usermanagement;uid=usermanagement_test;pwd=usermanagement_test;";
            var builder = new DbContextOptionsBuilder<TestContext>().UseMySQL(connectionString);
            var testContext = new TestContext(builder.Options);
            return testContext;
        }
    }
      public class MyDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        { 
            serviceCollection.AddEntityFrameworkMySQL(); 
        } 
    }
}
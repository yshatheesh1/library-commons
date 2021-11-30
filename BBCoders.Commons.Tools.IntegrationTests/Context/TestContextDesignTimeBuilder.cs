using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;

namespace BBCoders.Commons.Tools.IntegrationTests.Context
{
    public class TestContextDesignTimeBuilder : IDesignTimeDbContextFactory<TestContext>
    {
        public static string ConnectionString = "Server=localhost;port=3306;database=usermanagement;uid=usermanagement_test;pwd=usermanagement_test;";
        public TestContext CreateDbContext(string[] args)
        { 
            var builder = new DbContextOptionsBuilder<TestContext>().UseMySQL(ConnectionString);
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
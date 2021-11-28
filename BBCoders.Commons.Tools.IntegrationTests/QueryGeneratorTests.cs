using System;
using System.IO;
using Xunit;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class QueryGeneratorTests : IDisposable
    {
        public static string TestDirectory = "TestServices";
        public static string CurrentDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

        [Fact]
        public void Test()
        {
            var project = Project.FromFile(CurrentDirectory);
            new OperationExecutor(project).ExecuteQueryOperations();
        }
        
        public void Dispose()
        {
            Directory.Delete(Path.Combine(CurrentDirectory, TestDirectory), true);
        }

    }
}
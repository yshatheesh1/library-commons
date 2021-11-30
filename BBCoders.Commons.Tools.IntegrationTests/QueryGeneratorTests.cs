using System;
using System.IO;
using System.Threading.Tasks;
using BBCoders.Commons.Tools.IntegrationTests.Context;
using BBCoders.Example.DataServices;
using Xunit;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class QueryGeneratorTests
    {
        public static readonly string TestCurrentDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        [Fact]
        public async Task CreatesScheduleModelAndRepository()
        {
            var project = Project.FromFile(TestCurrentDirectory);
            var test = new QueryConfigurationTest1().GetQueryOptions();
            new OperationExecutor(project).ExecuteQueryOperations();

            // assert files are generated
            // Assert.True(Directory.Exists(test.OutputDirectory));
            // Assert.True(File.Exists(Path.Combine(test.OutputDirectory, test.ModelFileName + "." + test.FileExtension)));
            // Assert.True(File.Exists(Path.Combine(test.OutputDirectory, test.FileName + "." + test.FileExtension)));

            var repository = new ScheduleSiteRepository(TestContextDesignTimeBuilder.ConnectionString);
            var insert = new ScheduleSiteInsertModel()
            {
                IsActive = true,
                Name = "Test",
                ScheduleSiteId = Guid.NewGuid().ToByteArray()
            };
            var id = await repository.InsertScheduleSite(insert);
            // assert if data is inserted
            Assert.NotNull(id);
            var select = await repository.SelectScheduleSite(id);
            Assert.Equal(select.Id, id);
            Assert.Equal(select.IsActive, insert.IsActive);
            Assert.Equal(select.Name, insert.Name);
            Assert.Equal(select.ScheduleSiteId, insert.ScheduleSiteId);
            // assert update
            var update = new ScheduleSiteUpdateModel()
            {
                Id = select.Id,
                IsActive = false,
                Name = "Test2",
                ScheduleSiteId = select.ScheduleSiteId
            };
            await repository.UpdateScheduleSite(update);
            var updated = await repository.SelectScheduleSite(id);
            Assert.Equal(updated.Id, update.Id);
            Assert.Equal(updated.IsActive, update.IsActive);
            Assert.Equal(updated.Name, update.Name);
            Assert.Equal(updated.ScheduleSiteId, update.ScheduleSiteId);
            // assert delete
            var status = await repository.DeleteScheduleSite(updated.Id);
            Assert.Equal(status, 1);

        }

         [Fact]
        public async Task TestScheduleIncludesFingerprint()
        {

        }

    }
}
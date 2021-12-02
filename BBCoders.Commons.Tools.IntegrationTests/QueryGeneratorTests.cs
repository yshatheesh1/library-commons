using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BBCoders.Commons.Tools.IntegrationTests.Context;
using BBCoders.Example.DataServices;
using Xunit;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class QueryGeneratorTests
    {
        public static readonly string TestCurrentDirectory = Directory.GetCurrentDirectory();

        [Fact]
        public void ValidateGenerateFiles()
        {
            var test = new ScheduleSiteConfiguration().GetQueryOptions();

            // assert files are generated
            Assert.True(Directory.Exists(test.OutputDirectory));
            Assert.True(File.Exists(Path.Combine(test.OutputDirectory, test.ModelFileName + "." + test.FileExtension)));
            Assert.True(File.Exists(Path.Combine(test.OutputDirectory, test.FileName + "." + test.FileExtension)));
        }

        [Fact]
        public async Task CreatesScheduleModelAndRepository()
        {
            var repository = new ScheduleSiteRepository(TestContextDesignTimeBuilder.ConnectionString);
            var scheduleSiteModel = new ScheduleSiteModel()
            {
                IsActive = true,
                Name = "Test",
                ScheduleSiteId = Guid.NewGuid().ToByteArray()
            };
            await repository.InsertScheduleSite(scheduleSiteModel);
            // assert if data is inserted
            Assert.NotNull(scheduleSiteModel.Id);
            // assert update
            scheduleSiteModel.IsActive = false;
            scheduleSiteModel.Name = "Test2";
            scheduleSiteModel.ScheduleSiteId = Guid.NewGuid().ToByteArray();
            var updatedStatus = await repository.UpdateScheduleSite(scheduleSiteModel);
            Assert.Equal(1, updatedStatus);

            var updated = await repository.SelectScheduleSite(scheduleSiteModel.Id);
            Assert.Equal(updated.Id, scheduleSiteModel.Id);
            Assert.Equal(updated.IsActive, scheduleSiteModel.IsActive);
            Assert.Equal(updated.Name, scheduleSiteModel.Name);
            Assert.Equal(updated.ScheduleSiteId, scheduleSiteModel.ScheduleSiteId);
            // assert delete
            var deleteStatus = await repository.DeleteScheduleSite(updated.Id);
            Assert.Equal(1, deleteStatus);
        }

        [Fact]
        public async Task TestFingerprint()
        {
            var stateRepository = new StateRepository(TestContextDesignTimeBuilder.ConnectionString);
            var repository = new FingerprintRepository(TestContextDesignTimeBuilder.ConnectionString);
            var state = new StateModel()
            {
                Name = "Test",
                StateId = Guid.NewGuid().ToByteArray()
            };
            await stateRepository.InsertState(state);
            Assert.NotNull(state.Id);
            Assert.Equal(state.Name, "Test");
            var fingerprintModel = new FingerprintModel()
            {
                CreatedById = 1,
                FingerprintId = Guid.NewGuid().ToByteArray(),
                IsActive = true,
                LastUpdatedById = 1,
                NmlsId = 123,
                UpdatedDate = DateTime.Today,
                StateId = state.Id,
                CreatedDate = DateTime.Today
            };
            await repository.InsertFingerprint(fingerprintModel);
            Assert.NotNull(fingerprintModel.Id);
            Assert.Equal(fingerprintModel.CreatedById, 1);
            Assert.Equal(fingerprintModel.IsActive, true);
            Assert.Equal(fingerprintModel.LastUpdatedById, 1);
            Assert.Equal(fingerprintModel.NmlsId, 123);
            Assert.Equal(fingerprintModel.UpdatedDate, DateTime.Today);
            Assert.Equal(fingerprintModel.StateId, state.Id);
            Assert.Equal(fingerprintModel.CreatedDate, DateTime.Today);
            fingerprintModel.IsActive = false;
            fingerprintModel.NmlsId = 345;
            var updatedStatus = await repository.UpdateFingerprint(fingerprintModel);
            Assert.Equal(1, updatedStatus);
            var deleteFingerprintStatus = await repository.DeleteFingerprint(fingerprintModel.Id);
            Assert.Equal(1, deleteFingerprintStatus);
            var deleteStateStatus = await stateRepository.DeleteState(state.Id);
            Assert.Equal(1, deleteStateStatus);
        }

        [Fact]
        public async Task TestCustomQuery()
        {
            var repository = new ScheduleSiteRepository(TestContextDesignTimeBuilder.ConnectionString);
            var scheduleSiteModel = new ScheduleSiteModel()
            {
                IsActive = true,
                Name = "Test",
                ScheduleSiteId = Guid.NewGuid().ToByteArray()
            };
            await repository.InsertScheduleSite(scheduleSiteModel);
            var getScheduleSiteStatuses = await repository.GetSheduleSiteStatus(new GetSheduleSiteStatusRequestModel() { id = scheduleSiteModel.ScheduleSiteId });
            Assert.Equal(1, getScheduleSiteStatuses.Count);
            var getScheduleSiteStatus = getScheduleSiteStatuses.First();
            Assert.Equal(scheduleSiteModel.Id, getScheduleSiteStatus.ScheduleSiteId);
            Assert.Equal(scheduleSiteModel.Name, getScheduleSiteStatus.ScheduleSiteName);
            Assert.Equal(scheduleSiteModel.IsActive, getScheduleSiteStatus.ScheduleSiteIsActive);
            Assert.Equal(scheduleSiteModel.ScheduleSiteId, getScheduleSiteStatus.ScheduleSiteScheduleSiteId);

            var status = await repository.DeleteScheduleSite(scheduleSiteModel.Id);
            Assert.Equal(1, status);
        }


        [Fact]
        public async Task TestCustomQueryJoin()
        {
            var repository = new ScheduleSiteRepository(TestContextDesignTimeBuilder.ConnectionString);
            var scheduleSiteModel = new ScheduleSiteModel()
            {
                IsActive = true,
                Name = "Test",
                ScheduleSiteId = Guid.NewGuid().ToByteArray()
            };
            await repository.InsertScheduleSite(scheduleSiteModel);

            var actionRepository = new ActionRepository(TestContextDesignTimeBuilder.ConnectionString);
            var actionModel = await actionRepository.InsertAction(new ActionModel()
            {
                ActionId = Guid.NewGuid().ToByteArray(),
                Name = "test"
            });

            var scheduleRepository = new ScheduleRepository(TestContextDesignTimeBuilder.ConnectionString);
            var schedule = new ScheduleModel()
            {
                ScheduleId = Guid.NewGuid().ToByteArray(),
                CreatedById = 1,
                CreatedDate = DateTime.Today,
                LastUpdatedById = 1,
                LastUpdatedDate = DateTime.Today,
                ScheduleSiteId = scheduleSiteModel.Id,
                ScheduleDate = DateTime.Today,
                ActionId = actionModel.Id
            };

            await scheduleRepository.InsertSchedule(schedule);

            Assert.NotNull(schedule.Id);

            var customModels = await scheduleRepository.GetShedule(new GetSheduleRequestModel() { id = schedule.ScheduleId });
            Assert.Equal(1, customModels.Count);
            var customModel = customModels.First();
            Assert.Equal(customModel.ScheduleSiteName, scheduleSiteModel.Name);
            Assert.Equal(customModel.ScheduleSiteId, scheduleSiteModel.Id);
            Assert.Equal(customModel.ScheduleSiteIsActive, scheduleSiteModel.IsActive);
            Assert.Equal(customModel.ScheduleSiteScheduleSiteId, scheduleSiteModel.ScheduleSiteId);
            Assert.Equal(customModel.ScheduleId, schedule.Id);
            Assert.Equal(customModel.ScheduleActionId, schedule.ActionId);
            Assert.Equal(customModel.ScheduleCreatedById, schedule.CreatedById);
            Assert.Equal(customModel.ScheduleCreatedDate, schedule.CreatedDate);
            Assert.Equal(customModel.ScheduleFingerPrintId, schedule.FingerPrintId);
            Assert.Equal(customModel.ScheduleLastUpdatedById, schedule.LastUpdatedById);
            Assert.Equal(customModel.ScheduleLastUpdatedDate, schedule.LastUpdatedDate);
            Assert.Equal(customModel.ScheduleScheduleDate, schedule.ScheduleDate);
            Assert.Equal(customModel.ScheduleScheduleId, schedule.ScheduleId);
            Assert.Equal(customModel.ScheduleScheduleSiteId, schedule.ScheduleSiteId);

            var anotherSchedule = new ScheduleModel()
            {
                ScheduleId = Guid.NewGuid().ToByteArray(),
                CreatedById = 1,
                CreatedDate = DateTime.Today,
                LastUpdatedById = 1,
                LastUpdatedDate = DateTime.Today,
                ScheduleSiteId = scheduleSiteModel.Id,
                ScheduleDate = DateTime.Today,
                ActionId = actionModel.Id
            };
            await scheduleRepository.InsertSchedule(anotherSchedule);
            var getScheduleActionAndLocations = await scheduleRepository.GetScheduleActionAndLocation(new GetScheduleActionAndLocationRequestModel()
            {
                ActionId = actionModel.ActionId,
                LocationId = scheduleSiteModel.ScheduleSiteId
            });
            Assert.Equal(2, getScheduleActionAndLocations.Count);
            var getScheduleActionAndLocation = getScheduleActionAndLocations.First();
            Assert.Equal(getScheduleActionAndLocation.ScheduleSiteName, scheduleSiteModel.Name);
            Assert.Equal(getScheduleActionAndLocation.ScheduleSiteId, scheduleSiteModel.Id);
            Assert.Equal(getScheduleActionAndLocation.ScheduleSiteIsActive, scheduleSiteModel.IsActive);
            Assert.Equal(getScheduleActionAndLocation.ScheduleSiteScheduleSiteId, scheduleSiteModel.ScheduleSiteId);
            Assert.Equal(getScheduleActionAndLocation.ScheduleId, schedule.Id);
            Assert.Equal(getScheduleActionAndLocation.ScheduleActionId, schedule.ActionId);
            Assert.Equal(getScheduleActionAndLocation.ScheduleCreatedById, schedule.CreatedById);
            Assert.Equal(getScheduleActionAndLocation.ScheduleCreatedDate, schedule.CreatedDate);
            Assert.Equal(getScheduleActionAndLocation.ScheduleFingerPrintId, schedule.FingerPrintId);
            Assert.Equal(getScheduleActionAndLocation.ScheduleLastUpdatedById, schedule.LastUpdatedById);
            Assert.Equal(getScheduleActionAndLocation.ScheduleLastUpdatedDate, schedule.LastUpdatedDate);
            Assert.Equal(getScheduleActionAndLocation.ScheduleScheduleDate, schedule.ScheduleDate);
            Assert.Equal(getScheduleActionAndLocation.ScheduleScheduleId, schedule.ScheduleId);
            Assert.Equal(getScheduleActionAndLocation.ScheduleScheduleSiteId, schedule.ScheduleSiteId);
            Assert.Equal(getScheduleActionAndLocation.ActionId, actionModel.Id);
            Assert.Equal(getScheduleActionAndLocation.ActionName, actionModel.Name);
            Assert.Equal(getScheduleActionAndLocation.ActionActionId, actionModel.ActionId);


            await scheduleRepository.DeleteSchedule(schedule.Id);
            await scheduleRepository.DeleteSchedule(anotherSchedule.Id);
            await repository.DeleteScheduleSite(scheduleSiteModel.Id);
            await actionRepository.DeleteAction(actionModel.Id);
        }

    }
}
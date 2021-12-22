using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BBCoders.Commons.Tools.IntegrationTests.Context;
using BBCoders.Example.DataServices;
using MySql.Data.MySqlClient;
using Xunit;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class QueryGeneratorTests
    {
        public static readonly string TestCurrentDirectory = Directory.GetCurrentDirectory();

        [Fact]
        public async Task CreatesScheduleModelAndRepository()
        {
            var repository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
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

            var updated = await repository.SelectScheduleSite(new ScheduleSiteKey() { Id = scheduleSiteModel.Id });
            Assert.Equal(updated.Id, scheduleSiteModel.Id);
            Assert.Equal(updated.IsActive, scheduleSiteModel.IsActive);
            Assert.Equal(updated.Name, scheduleSiteModel.Name);
            Assert.Equal(updated.ScheduleSiteId, scheduleSiteModel.ScheduleSiteId);
            // assert delete
            var deleteStatus = await repository.DeleteScheduleSite(new ScheduleSiteKey() { Id = updated.Id });
            Assert.Equal(1, deleteStatus);
        }

        [Fact]
        public async Task TestFingerprint()
        {
            var stateRepository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var repository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
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
            var deleteFingerprintStatus = await repository.DeleteFingerprint(new FingerprintKey() { Id = fingerprintModel.Id });
            Assert.Equal(1, deleteFingerprintStatus);
            var deleteStateStatus = await stateRepository.DeleteState(new StateKey() { Id = state.Id });
            Assert.Equal(1, deleteStateStatus);
        }

        [Fact]
        public async Task TestCustomQuery()
        {
            var repository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
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
            Assert.Equal(scheduleSiteModel.Id, getScheduleSiteStatus.ScheduleSite.Id);
            Assert.Equal(scheduleSiteModel.Name, getScheduleSiteStatus.ScheduleSite.Name);
            Assert.Equal(scheduleSiteModel.IsActive, getScheduleSiteStatus.ScheduleSite.IsActive);
            Assert.Equal(scheduleSiteModel.ScheduleSiteId, getScheduleSiteStatus.ScheduleSite.ScheduleSiteId);

            var status = await repository.DeleteScheduleSite(new ScheduleSiteKey() { Id = scheduleSiteModel.Id });
            Assert.Equal(1, status);
        }


        [Fact]
        public async Task TestCustomQueryJoin()
        {
            var repository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var scheduleSiteModel = new ScheduleSiteModel()
            {
                IsActive = true,
                Name = "Test",
                ScheduleSiteId = Guid.NewGuid().ToByteArray()
            };
            await repository.InsertScheduleSite(scheduleSiteModel);

            var actionRepository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var actionModel = await actionRepository.InsertAction(new ActionModel()
            {
                ActionId = Guid.NewGuid().ToByteArray(),
                Name = "test"
            });

            var scheduleRepository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
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
            Assert.Equal(customModel.ScheduleSite.Name, scheduleSiteModel.Name);
            Assert.Equal(customModel.ScheduleSite.Id, scheduleSiteModel.Id);
            Assert.Equal(customModel.ScheduleSite.IsActive, scheduleSiteModel.IsActive);
            Assert.Equal(customModel.ScheduleSite.ScheduleSiteId, scheduleSiteModel.ScheduleSiteId);
            Assert.Equal(customModel.Schedule.Id, schedule.Id);
            Assert.Equal(customModel.Schedule.ActionId, schedule.ActionId);
            Assert.Equal(customModel.Schedule.CreatedById, schedule.CreatedById);
            Assert.Equal(customModel.Schedule.CreatedDate, schedule.CreatedDate);
            Assert.Equal(customModel.Schedule.FingerPrintId, schedule.FingerPrintId);
            Assert.Equal(customModel.Schedule.LastUpdatedById, schedule.LastUpdatedById);
            Assert.Equal(customModel.Schedule.LastUpdatedDate, schedule.LastUpdatedDate);
            Assert.Equal(customModel.Schedule.ScheduleDate, schedule.ScheduleDate);
            Assert.Equal(customModel.Schedule.ScheduleId, schedule.ScheduleId);
            Assert.Equal(customModel.Schedule.ScheduleSiteId, schedule.ScheduleSiteId);

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
            Assert.Equal(getScheduleActionAndLocation.ScheduleSite.Name, scheduleSiteModel.Name);
            Assert.Equal(getScheduleActionAndLocation.ScheduleSite.Id, scheduleSiteModel.Id);
            Assert.Equal(getScheduleActionAndLocation.ScheduleSite.IsActive, scheduleSiteModel.IsActive);
            Assert.Equal(getScheduleActionAndLocation.ScheduleSite.ScheduleSiteId, scheduleSiteModel.ScheduleSiteId);
            Assert.Equal(getScheduleActionAndLocation.Schedule.Id, schedule.Id);
            Assert.Equal(getScheduleActionAndLocation.Schedule.ActionId, schedule.ActionId);
            Assert.Equal(getScheduleActionAndLocation.Schedule.CreatedById, schedule.CreatedById);
            Assert.Equal(getScheduleActionAndLocation.Schedule.CreatedDate, schedule.CreatedDate);
            Assert.Equal(getScheduleActionAndLocation.Schedule.FingerPrintId, schedule.FingerPrintId);
            Assert.Equal(getScheduleActionAndLocation.Schedule.LastUpdatedById, schedule.LastUpdatedById);
            Assert.Equal(getScheduleActionAndLocation.Schedule.LastUpdatedDate, schedule.LastUpdatedDate);
            Assert.Equal(getScheduleActionAndLocation.Schedule.ScheduleDate, schedule.ScheduleDate);
            Assert.Equal(getScheduleActionAndLocation.Schedule.ScheduleId, schedule.ScheduleId);
            Assert.Equal(getScheduleActionAndLocation.Schedule.ScheduleSiteId, schedule.ScheduleSiteId);
            Assert.Equal(getScheduleActionAndLocation.Action.Id, actionModel.Id);
            Assert.Equal(getScheduleActionAndLocation.Action.Name, actionModel.Name);
            Assert.Equal(getScheduleActionAndLocation.Action.ActionId, actionModel.ActionId);


            await scheduleRepository.DeleteSchedule(new ScheduleKey() { Id = schedule.Id });
            await scheduleRepository.DeleteSchedule(new ScheduleKey() { Id = anotherSchedule.Id });
            await repository.DeleteScheduleSite(new ScheduleSiteKey() { Id = scheduleSiteModel.Id });
            await actionRepository.DeleteAction(new ActionKey() { Id = actionModel.Id });
        }

        [Fact]
        public async Task TestCustomInQuery()
        {
            var stateRepository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var state = new StateModel()
            {
                Name = "Test",
                StateId = Guid.NewGuid().ToByteArray()
            };
            await stateRepository.InsertState(state);
            Assert.NotNull(state.Id);
            Assert.Equal(state.Name, "Test");
            var repository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var fingerprintModel1 = new FingerprintModel()
            {
                CreatedById = 1,
                FingerprintId = Guid.NewGuid().ToByteArray(),
                IsActive = true,
                LastUpdatedById = 1,
                NmlsId = 123,
                UpdatedDate = DateTime.Today,
                CreatedDate = DateTime.Today,
                StateId = state.Id
            };
            var fingerprintModel2 = new FingerprintModel()
            {
                CreatedById = 1,
                FingerprintId = Guid.NewGuid().ToByteArray(),
                IsActive = true,
                LastUpdatedById = 1,
                NmlsId = 123,
                UpdatedDate = DateTime.Today,
                CreatedDate = DateTime.Today,
                StateId = state.Id
            };
            await repository.InsertFingerprint(fingerprintModel1);
            await repository.InsertFingerprint(fingerprintModel2);

            var response = await repository.GetFingerprintByGuids(new GetFingerprintByGuidsRequestModel()
            {
                test = new List<Byte[]>() { fingerprintModel1.FingerprintId, fingerprintModel2.FingerprintId }
            });
            Assert.Equal(2, response.Count);
            await repository.DeleteFingerprint(new FingerprintKey() { Id =  fingerprintModel1.Id });
            await repository.DeleteFingerprint(new FingerprintKey() { Id =  fingerprintModel2.Id });
        }
    }
}
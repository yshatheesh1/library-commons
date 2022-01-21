using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BBCoders.Commons.Tools.IntegrationTests.Context;
using MySql.Data.MySqlClient;
using BBCoders.Example.DataModels;
using BBCoders.Example.DataServices;
using Xunit;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class QueryGeneratorTests
    {
        public static readonly string TestCurrentDirectory = Directory.GetCurrentDirectory();
        public static readonly string GoTestCurrentDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "GoIntegrationTests");

        [Fact]
        public async Task CreatesScheduleModelAndRepository()
        {
            var repository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var scheduleSiteModel = new ScheduleSiteModel()
            {
                IsActive = true,
                Name = "Test",
                ScheduleSiteId = Guid.NewGuid()
            };
            scheduleSiteModel = await repository.InsertScheduleSite(scheduleSiteModel);
            // assert if data is inserted
            Assert.NotNull(scheduleSiteModel.Id);
            // assert update
            scheduleSiteModel.IsActive = false;
            scheduleSiteModel.Name = "Test2";
            scheduleSiteModel.ScheduleSiteId = Guid.NewGuid();
            var updatedStatus = await repository.UpdateScheduleSite(scheduleSiteModel);
            Assert.Equal(updatedStatus.Id, scheduleSiteModel.Id);
            Assert.Equal(updatedStatus.Name, scheduleSiteModel.Name);
            Assert.Equal(updatedStatus.IsActive, scheduleSiteModel.IsActive);
            Assert.Equal(updatedStatus.ScheduleSiteId, scheduleSiteModel.ScheduleSiteId);

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
                StateId = Guid.NewGuid()
            };
            state = await stateRepository.InsertState(state);
            Assert.NotNull(state.Id);
            Assert.Equal(state.Name, "Test");
            var fingerprintModel = new FingerprintModel()
            {
                CreatedById = 1,
                FingerprintId = Guid.NewGuid(),
                IsActive = true,
                LastUpdatedById = 1,
                NmlsId = 123,
                UpdatedDate = DateTime.Today,
                StateId = state.Id,
                CreatedDate = DateTime.Today
            };
            fingerprintModel = await repository.InsertFingerprint(fingerprintModel);
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
            Assert.Equal(updatedStatus.Id, fingerprintModel.Id);
            Assert.Equal(updatedStatus.CreatedById, fingerprintModel.CreatedById);
            Assert.Equal(updatedStatus.IsActive, fingerprintModel.IsActive);
            Assert.Equal(updatedStatus.LastUpdatedById, fingerprintModel.LastUpdatedById);

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
                ScheduleSiteId = Guid.NewGuid()
            };
            scheduleSiteModel = await repository.InsertScheduleSite(scheduleSiteModel);
            var getScheduleSiteStatuses = await repository.GetSheduleSiteStatus(new GetSheduleSiteStatusRequestModel() { id = scheduleSiteModel.ScheduleSiteId });

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
                ScheduleSiteId = Guid.NewGuid()
            };
            scheduleSiteModel = await repository.InsertScheduleSite(scheduleSiteModel);

            var actionRepository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var actionModel = await actionRepository.InsertAction(new ActionModel()
            {
                ActionId = Guid.NewGuid(),
                Name = "test"
            });

            var scheduleRepository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var schedule = new ScheduleModel()
            {
                ScheduleId = Guid.NewGuid(),
                CreatedById = 1,
                CreatedDate = DateTime.Today,
                LastUpdatedById = 1,
                LastUpdatedDate = DateTime.Today,
                ScheduleSiteId = scheduleSiteModel.Id,
                ScheduleDate = DateTime.Today,
                ActionId = actionModel.Id
            };

            schedule = await scheduleRepository.InsertSchedule(schedule);

            Assert.NotNull(schedule.Id);

            var customModels = await scheduleRepository.GetShedule(new GetSheduleRequestModel() { id = schedule.ScheduleId });
            Assert.Equal(1, customModels.Count);
            var customModel = customModels.First();
            // Assert.Equal(customModel.ScheduleSite.Name, scheduleSiteModel.Name);
            // Assert.Equal(customModel.ScheduleSite.Id, scheduleSiteModel.Id);
            // Assert.Equal(customModel.ScheduleSite.IsActive, scheduleSiteModel.IsActive);
            // Assert.Equal(customModel.ScheduleSite.ScheduleSiteId, scheduleSiteModel.ScheduleSiteId);
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
                ScheduleId = Guid.NewGuid(),
                CreatedById = 1,
                CreatedDate = DateTime.Today,
                LastUpdatedById = 1,
                LastUpdatedDate = DateTime.Today,
                ScheduleSiteId = scheduleSiteModel.Id,
                ScheduleDate = DateTime.Today,
                ActionId = actionModel.Id
            };
            anotherSchedule = await scheduleRepository.InsertSchedule(anotherSchedule);
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
        public async Task TestBatchQueries()
        {
            var stateRepository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var state = new StateModel()
            {
                Name = "Test",
                StateId = Guid.NewGuid()
            };
            state = await stateRepository.InsertState(state);
            Assert.NotNull(state.Id);
            Assert.Equal(state.Name, "Test");
            var repository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            var fingerprintModel1 = new FingerprintModel()
            {
                CreatedById = 1,
                FingerprintId = Guid.NewGuid(),
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
                FingerprintId = Guid.NewGuid(),
                IsActive = true,
                LastUpdatedById = 1,
                NmlsId = 123,
                UpdatedDate = DateTime.Today,
                CreatedDate = DateTime.Today,
                StateId = state.Id
            };
            var fingerprintModels = await repository.InsertBatchFingerprint(new List<FingerprintModel>() { fingerprintModel1, fingerprintModel2 });
            fingerprintModel1 = fingerprintModels[0];
            fingerprintModel2 = fingerprintModels[1];

            var response = await repository.GetFingerprintByGuids(new GetFingerprintByGuidsRequestModel()
            {
                test = new List<Guid>() { fingerprintModel1.FingerprintId, fingerprintModel2.FingerprintId }
            });
            Assert.Equal(2, response.Count);
            fingerprintModels = await repository.UpdateBatchFingerprint(new List<FingerprintModel>() { fingerprintModel1, fingerprintModel2 });
            Assert.Equal(2, fingerprintModels.Count);

            await repository.DeleteBatchFingerprint(new List<FingerprintKey>(){
                new FingerprintKey() { Id = fingerprintModel1.Id },
               new FingerprintKey() { Id = fingerprintModel2.Id }
            });
        }

        [Fact]
        public async Task testCompositeKey()
        {
            // test batch statements
            var respository = new MySqlConnection(TestContextDesignTimeBuilder.ConnectionString);
            await respository.OpenAsync();
            var transaction = respository.BeginTransaction();
            try
            {
                var statuses = new List<StatusModel>(){
                new StatusModel(){Id1 = 1, Id2 = 1,StatusId = Guid.NewGuid(), Description = "test1"},
                new StatusModel() {Id1 = 2,Id2 = 2, StatusId = Guid.NewGuid(), Description = "test2"}};

                var insertedRecords = await respository.InsertBatchStatuses(statuses, transaction);
                Assert.Equal(2, insertedRecords.Count);
                Assert.NotNull(insertedRecords[0].Id1);
                Assert.NotNull(insertedRecords[0].StatusId);
                Assert.Equal(1, insertedRecords[0].Id2);
                Assert.Equal("test1", insertedRecords[0].Description);
                Assert.NotNull(insertedRecords[1].Id1);
                Assert.Equal(2, insertedRecords[1].Id2);
                Assert.NotNull(insertedRecords[1].StatusId);
                Assert.Equal("test2", insertedRecords[1].Description);

                var selectRecords = await respository.SelectBatchStatuses(new List<StatusKey>(){
                new StatusKey() { Id1 = insertedRecords[0].Id1, Id2 = insertedRecords[0].Id2 },
                new StatusKey() { Id1 = insertedRecords[1].Id1, Id2 = insertedRecords[1].Id2 }});
                Assert.Equal(2, selectRecords.Count);
                Assert.Equal(insertedRecords[0].Id1, selectRecords[0].Id1);
                Assert.Equal(insertedRecords[1].Id1, selectRecords[1].Id1);

                var updatedRecords = await respository.UpdateBatchStatuses(
                    insertedRecords.Select(x => { x.Description = "new" + x.Description; return x; }).ToList(),
                    transaction);

                Assert.Equal(2, updatedRecords.Count);
                Assert.Equal("newtest1", updatedRecords[0].Description);
                Assert.Equal("newtest2", updatedRecords[1].Description);

                var deleteRecords = await respository.DeleteBatchStatuses(new List<StatusKey>(){
                new StatusKey() { Id1 = insertedRecords[0].Id1, Id2 = insertedRecords[0].Id2 },
                new StatusKey() { Id1 = insertedRecords[1].Id1, Id2 = insertedRecords[1].Id2 }}, transaction);
                Assert.Equal(2, deleteRecords);

                // test non batch statements
                var status = new StatusModel() { Id1 = 3, Id2 = 3, StatusId = Guid.NewGuid(), Description = "test3" };
                var insertAnotherRecord = await respository.InsertStatuses(status, transaction);
                Assert.NotNull(insertAnotherRecord.Id1);
                Assert.NotNull(insertAnotherRecord.StatusId);
                Assert.Equal(3, insertAnotherRecord.Id2);
                Assert.Equal("test3", insertAnotherRecord.Description);

                var selectRecord = await respository.SelectStatuses(new StatusKey() { Id1 = insertAnotherRecord.Id1, Id2 = insertAnotherRecord.Id2 });
                Assert.NotNull(selectRecord);
                Assert.Equal(selectRecord.Id1, insertAnotherRecord.Id1);
                Assert.Equal(selectRecord.Id2, insertAnotherRecord.Id2);

                insertAnotherRecord.Description = "newtest3";
                var updateRecord = await respository.UpdateStatuses(insertAnotherRecord, transaction);
                Assert.Equal(updateRecord.Description, "newtest3");

                var deleteRecord = await respository.DeleteStatuses(new StatusKey() { Id1 = insertAnotherRecord.Id1, Id2 = insertAnotherRecord.Id2 }, transaction);
                Assert.Equal(1, deleteRecord);
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw e;
            }
        }
    }
}
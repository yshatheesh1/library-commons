using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BBCoders.Commons.QueryGenerator;
using BBCoders.Commons.Tools.IntegrationTests.Context;
using Microsoft.EntityFrameworkCore;
using Action = BBCoders.Commons.Tools.IntegrationTests.Context.Action;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class ScheduleSiteConfiguration : IQueryGenerator<TestContext>
    {
        public void CreateQuery(TestContext context, QueryContext queryOperations)
        {
            queryOperations.Add<ScheduleSite>();
            queryOperations.Add<string>("GetScheduleSitesByLocation", (location) => context.ScheduleSites.Where(x => EF.Functions.Like(x.Name, $"%{location}%")));
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
    public class FingerprintConfiguration : IQueryGenerator<TestContext>
    {
        public void CreateQuery(TestContext context, QueryContext queryOperations)
        {
            queryOperations.Add<Fingerprint>();
            queryOperations.Add<List<Guid>>("GetFingerprintByGuids", (test) => context.Fingerprints.Where(x => test.Contains(x.FingerprintId)));
            queryOperations.Add<List<long>>("GetFingerprintsById", (test) => context.Fingerprints.Where(x => test.Contains(x.Id)).Select(x => new { Id = x.Id, FingerprintId = x.FingerprintId, IsActive = x.IsActive }));
            queryOperations.Add<List<Guid>, Boolean, List<Guid>>("GetFingerprintByStateId", (fingerprintId, active, stateId) =>
            context.Fingerprints.Include(x => x.State).Where(x => fingerprintId.Contains(x.FingerprintId) && x.IsActive == active && stateId.Contains(x.State.StateId)));
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

    public class Stateconfiguration : IQueryGenerator<TestContext>
    {
        public void CreateQuery(TestContext context, QueryContext queryOperations)
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

    public class ScheduleConfiguration : IQueryGenerator<TestContext>
    {
        public void CreateQuery(TestContext context, QueryContext queryOperations)
        {
            queryOperations.Add<Schedule>();
            queryOperations.Add<Guid>("GetSheduleAction", (id) => context.Schedules.Include(x => x.scheduleSite).Where(x => x.ScheduleId == id).Select(x => new { action = x.ActionId, id = x.Id, schedule_id = x.ScheduleId }));
            queryOperations.Add<Guid>("GetShedule", (id) => context.Schedules.Include(x => x.scheduleSite).Where(x => x.ScheduleId == id));
            queryOperations.Add<Guid, Guid>("GetScheduleActionAndLocation", (ActionId, LocationId) =>
                context.Schedules.Join(context.Actions, schedule => schedule.ActionId, action => action.Id, (schedule, action) => new { schedule, action })
                .Join(context.ScheduleSites, schedule_action => schedule_action.schedule.ScheduleSiteId, schedulesite => schedulesite.Id, (schedule_action, schedulesite) => new { schedule_action, schedulesite })
                .Where(x => x.schedule_action.action.ActionId == ActionId && x.schedulesite.ScheduleSiteId == LocationId));

            queryOperations.Add<Guid>("GetSheduleAsync", (id) => 
            context.Schedules
                .Include(x => x.scheduleSite)
                .Include(x => x.Fingerprint)
                .Include(x => x.Fingerprint.State).Where(x => x.ScheduleId == id).Select(x => new { Id = x.Id, Test = x.CreatedById, StateActive = true, state = x.Fingerprint.State }));
            queryOperations.Add<Guid>("GetSheduleAsync2", (id) => 
            context.Schedules
                .GroupBy(x => x.Id, (x, y)  =>  y.Count() ));
                
        }

        public QueryOptions GetQueryOptions()
        {
            return new QueryOptions()
            {
                ClassName = "ScheduleRepository",
                Language = "csharp",
                OutputDirectory = Path.Combine(QueryGeneratorTests.TestCurrentDirectory, "TestServices"),
                PackageName = "BBCoders.Example.DataServices",
                FileExtension = "cs",
                FileName = "ScheduleRepository",
                ModelFileName = "ScheduleDataModel"
            };
        }

        public class ActionConfiguration : IQueryGenerator<TestContext>
        {
            public void CreateQuery(TestContext context, QueryContext queryOperations)
            {
                queryOperations.Add<Action>();
            }

            public QueryOptions GetQueryOptions()
            {
                return new QueryOptions()
                {
                    ClassName = "ActionRepository",
                    Language = "csharp",
                    OutputDirectory = Path.Combine(QueryGeneratorTests.TestCurrentDirectory, "TestServices"),
                    PackageName = "BBCoders.Example.DataServices",
                    FileExtension = "cs",
                    FileName = "ActionRepository",
                    ModelFileName = "ActionModel"
                };
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BBCoders.Commons.QueryConfiguration;
using BBCoders.Commons.Tools.IntegrationTests.Context;
using Microsoft.EntityFrameworkCore;
using Action = BBCoders.Commons.Tools.IntegrationTests.Context.Action;

namespace BBCoders.Commons.Tools.IntegrationTests
{
    public class ScheduleSiteConfiguration : IQueryConfiguration<TestContext>
    {
        public void CreateQuery(TestContext context, QueryOperations queryOperations)
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
    public class FingerprintConfiguration : IQueryConfiguration<TestContext>
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

    public class Stateconfiguration : IQueryConfiguration<TestContext>
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

    public class ScheduleConfiguration : IQueryConfiguration<TestContext>
    {
        public void CreateQuery(TestContext context, QueryOperations queryOperations)
        {
            queryOperations.Add<Schedule>();
            queryOperations.Add<Guid>("GetSheduleAction", (id) => context.Schedules.Include(x => x.scheduleSite).Where(x => x.ScheduleId == id).Select(x => new { action = x.ActionId, id = x.Id, schedule_id = x.ScheduleId }));
            queryOperations.Add<Guid>("GetShedule", (id) => context.Schedules.Include(x => x.scheduleSite).Where(x => x.ScheduleId == id));
            queryOperations.Add<Guid, Guid>("GetScheduleActionAndLocation", (ActionId, LocationId) =>
                context.Schedules.Join(context.Actions, schedule => schedule.ActionId, action => action.Id, (schedule, action) => new { schedule, action })
                .Join(context.ScheduleSites, schedule_action => schedule_action.schedule.ScheduleSiteId, schedulesite => schedulesite.Id, (schedule_action, schedulesite) => new { schedule_action, schedulesite })
                .Where(x => x.schedule_action.action.ActionId == ActionId && x.schedulesite.ScheduleSiteId == LocationId));
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

        public class ActionConfiguration : IQueryConfiguration<TestContext>
        {
            public void CreateQuery(TestContext context, QueryOperations queryOperations)
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
using Microsoft.EntityFrameworkCore;

namespace BBCoders.Commons.Tools.IntegrationTests.Context
{
    public class TestContext : DbContext
    {
        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<Fingerprint> Fingerprints { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<ScheduleSite> ScheduleSites { get; set; }
        public TestContext(DbContextOptions<TestContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Fingerprint>().Property(d => d.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP()");
        }
    }
}
using APSIM.POStats.Shared;
using APSIM.POStats.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace APSIM.POStats.Portal.Data
{
    /// <summary>
    /// Stats database context.
    /// </summary>
    public class StatsDbContext : DbContext
    {
        public DbSet<PullRequest> PullRequests { get; set; }
        public DbSet<ApsimFile> ApsimFiles { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Variable> Variables { get; set; }
        public StatsDbContext(DbContextOptions<StatsDbContext> options)
         : base(options)
        {
        }

        /// <summary>
        /// Override model creating event so that we can convert the plural property
        /// names (e.g. PullRequests) into singular DB table names (e.g. PullRequest)
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PullRequest>().ToTable("PullRequest");
            modelBuilder.Entity<ApsimFile>().ToTable("ApsimFile");
            modelBuilder.Entity<Table>().ToTable("Table");
            modelBuilder.Entity<Variable>().ToTable("Variable");
        }
    }
}

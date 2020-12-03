using APSIM.POStats.Migrate.OldModels;
using Microsoft.EntityFrameworkCore;

namespace APSIM.POStats.Migrate.OldData
{
    public partial class OldStatsDbContext : DbContext
    {
        public OldStatsDbContext(DbContextOptions<OldStatsDbContext> options)
                : base(options)
        {
        }

        //these are in the database
        public virtual DbSet<AcceptStatsLogs> AcceptStatsLogs { get; set; }
        public virtual DbSet<ApsimFiles> ApsimFiles { get; set; }
        public virtual DbSet<PredictedObservedDetails> PredictedObservedDetails { get; set; }
        public virtual DbSet<PredictedObservedTests> PredictedObservedTests { get; set; }
        public virtual DbSet<PredictedObservedValues> PredictedObservedValues { get; set; }
        public virtual DbSet<Simulations> Simulations { get; set; }
    }
}
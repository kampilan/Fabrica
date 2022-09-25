using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Utilities.Container;
using Fabrica.Work.Persistence.Entities;
using Fabrica.Work.Persistence.Modelers;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Work.Persistence.Contexts;

public class ExplorerDbContext: ReplicaDbContext
{

    public ExplorerDbContext([NotNull] ICorrelation correlation, [NotNull] DbContextOptions options, [NotNull] ILoggerFactory factory) : base(correlation, options, factory)
    {
    }

    public DbSet<WorkTopic> WorkTopics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        
        new WorkTopicModeler().Configure(builder.Entity<WorkTopic>());

    }


}
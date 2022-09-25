using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Work.Persistence.Entities;
using Fabrica.Work.Persistence.Modelers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Work.Persistence.Contexts;

public class WorkDbContext: OriginDbContext
{

    public WorkDbContext( ICorrelation correlation, IRuleSet rules, DbContextOptions options, ILoggerFactory factory) : base(correlation, rules, options, factory)
    {
    }

    public DbSet<WorkTopic> WorkTopics { get; set; } = null!;

    protected override void OnModelCreating( ModelBuilder builder )
    {
        new WorkTopicModeler().Configure(builder.Entity<WorkTopic>());
    }

}
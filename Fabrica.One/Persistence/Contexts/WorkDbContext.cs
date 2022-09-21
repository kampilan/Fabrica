using Fabrica.One.Persistence.Modelers;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Work.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.One.Persistence.Contexts;

public class WorkDbContext: OriginDbContext
{

    public WorkDbContext([NotNull] ICorrelation correlation, [NotNull] IRuleSet rules, [NotNull] DbContextOptions options, [NotNull] ILoggerFactory factory) : base(correlation, rules, options, factory)
    {
    }

    public DbSet<WorkTopic> WorkTopics { get; set; }

    protected override void OnModelCreating( ModelBuilder builder )
    {
        new WorkTopicModeler().Configure(builder.Entity<WorkTopic>());
    }

}
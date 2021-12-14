using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Fake.Persistence;

public class FakeOriginDbContext: OriginDbContext
{

    public FakeOriginDbContext(ICorrelation correlation, IRuleSet rules, [NotNull] DbContextOptions options, ILoggerFactory factory = null) : base(correlation, rules, options, factory)
    {

    }


    public DbSet<Company> Companies { get; set; }
    public DbSet<Person> People { get; set; }




}
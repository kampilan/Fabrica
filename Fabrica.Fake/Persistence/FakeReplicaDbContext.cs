using System;
using System.Collections.Generic;
using Bogus;
using Bogus.DataSets;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Fake.Persistence;

public class FakeReplicaDbContext: ReplicaDbContext
{



    public FakeReplicaDbContext(ICorrelation correlation, DbContextOptions options, ILoggerFactory factory = null) : base(correlation, options, factory)
    {
    }


    public DbSet<Company> Companies { get; set; }
    public DbSet<Person> People { get; set; }


}
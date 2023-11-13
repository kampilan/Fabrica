using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Persistence.Ef.Contexts;

public class ReplicaDbContextOptionsBuilder: DbContextOptionsBuilder
{

    public ReplicaDbContextOptionsBuilder(ICorrelation correlation, ILoggerFactory factory)
    {

        Correlation = correlation;
        LoggerFactory = factory;

    }

    public ICorrelation Correlation { get; }
    public ILoggerFactory LoggerFactory { get; }


}
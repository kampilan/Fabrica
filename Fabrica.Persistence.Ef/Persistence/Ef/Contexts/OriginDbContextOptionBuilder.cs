using Fabrica.Persistence.Connection;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Persistence.Ef.Contexts;

public class OriginDbContextOptionBuilder: DbContextOptionsBuilder
{

    public OriginDbContextOptionBuilder(ICorrelation correlation, IRuleSet rules, IConnectionResolver resolver, IUnitOfWork uow, ILoggerFactory factory)
    {

        Correlation = correlation;
        Rules = rules;
        Resolver = resolver;
        Uow = uow;
        Factory = factory;

    }    
    
    
    public ICorrelation Correlation { get; }
    public IRuleSet Rules { get; }
    public IConnectionResolver Resolver { get; }
    public IUnitOfWork Uow { get; }
    public ILoggerFactory Factory { get; }


    public void ConfigureDbContext( OriginDbContext context )
    {
        context.Database.UseTransaction(Uow.Transaction);
    }


}
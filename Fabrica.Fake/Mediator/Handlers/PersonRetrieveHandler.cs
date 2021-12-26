using System;
using System.Linq;
using Fabrica.Fake.Persistence;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;

namespace Fabrica.Fake.Mediator.Handlers;

public class PersonRetrieveHandler: BaseRetrieveHandler<RetrieveEntityRequest<Person>,Person, FakeOriginDbContext>
{

    public PersonRetrieveHandler(ICorrelation correlation, FakeOriginDbContext context) : base(correlation, context)
    {
    }

    protected override Func<FakeOriginDbContext, IQueryable<Person>> One => c => c.People;


}
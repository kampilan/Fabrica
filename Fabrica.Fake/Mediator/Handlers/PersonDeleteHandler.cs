using System;
using System.Linq;
using Fabrica.Fake.Persistence;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;

namespace Fabrica.Fake.Mediator.Handlers;

public class PersonDeleteHandler: BaseDeleteHandler<DeleteEntityRequest<Person>,Person,FakeOriginDbContext>
{

    public PersonDeleteHandler(ICorrelation correlation, IUnitOfWork uow, FakeOriginDbContext context) : base(correlation, uow, context)
    {
    }

    protected override Func<FakeOriginDbContext, IQueryable<Person>> One => c => c.People;

}
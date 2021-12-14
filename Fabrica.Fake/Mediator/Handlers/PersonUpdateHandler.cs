
using System;
using System.Linq;
using AutoMapper;
using Fabrica.Fake.Persistence;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;

namespace Fabrica.Fake.Mediator.Handlers;

public class PersonUpdateHandler: BaseUpdateHandler<UpdateEntityRequest<Person>,Person,FakeOriginDbContext>
{

    public PersonUpdateHandler(ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, FakeOriginDbContext context, IMapper mapper) : base(correlation, meta, uow, context, mapper)
    {
    }

    protected override Func<FakeOriginDbContext, IQueryable<Person>> One => c => c.People;

}
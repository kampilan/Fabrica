using System;
using System.Linq;
using Fabrica.Fake.Persistence;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Rules;
using Fabrica.Utilities.Container;

namespace Fabrica.Fake.Mediator.Handlers;

public class PersonQueryHandler: BaseQueryHandler<QueryEntityRequest<Person>,Person,FakeReplicaDbContext>
{


    public PersonQueryHandler(ICorrelation correlation, IRuleSet rules, FakeReplicaDbContext context) : base(correlation, rules, context)
    {
    }

    protected override Func<FakeReplicaDbContext, IQueryable<Person>> Many => c => c.People;


}
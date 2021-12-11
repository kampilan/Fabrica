using AutoMapper;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Test.Models.Patch;
using Fabrica.Utilities.Container;

namespace Fabrica.Test.Models.Handlers;

public class UpdatePersonHandler: BaseHandler<UpdateEntityRequest<Person>,Person>
{

    public UpdatePersonHandler(ICorrelation correlation, IMapper mapper) : base(correlation)
    {

        Mapper = mapper;

    }

    private IMapper Mapper { get; }

    protected override Task<Person> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();

        var entity = new Person();

        Mapper.Map(Request.Delta, entity);

        return Task.FromResult(entity);

    }


}
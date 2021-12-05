using AutoMapper;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Mediator.Handlers;
using Fabrica.Utilities.Container;

namespace Fabrica.Test.Models.Handlers;

public class CreateMemberEntityHandler<TParent,TMember>: BaseHandler<CreateMemberEntityRequest<TParent,TMember>,TMember> where TParent: class, IModel where TMember: class, IModel, new()
{


    public CreateMemberEntityHandler(ICorrelation correlation, IMapper mapper ) : base( correlation )
    {

        Mapper = mapper;

    }

    private IMapper Mapper { get; }

    protected override Task<TMember> Perform( CancellationToken cancellationToken = default )
    {

        using var logger = EnterMethod();

        var entity = new TMember();

        Mapper.Map(Request.Delta, entity);

        return Task.FromResult(entity);

    }


}
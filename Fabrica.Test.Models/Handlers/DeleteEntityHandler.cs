using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;

namespace Fabrica.Test.Models.Handlers;

public class DeleteEntityHandler<TEntity>: BaseHandler<DeleteEntityRequest<TEntity>> where TEntity: class, IModel
{

    public DeleteEntityHandler(ICorrelation correlation) : base(correlation)
    {
    }

    protected override Task Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        return Task.CompletedTask;

    }

}
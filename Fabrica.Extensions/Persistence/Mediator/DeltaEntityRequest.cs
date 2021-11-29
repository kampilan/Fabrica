using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class DeltaEntityRequest<TEntity,TDelta>: IDeltaEntityRequest, IRequest<Response<TEntity>> where TEntity: class, IModel where TDelta: BaseDelta, new()
{

    public OperationType Operation { get; set; }

    public string Uid { get; set; } = "";

    public TDelta Delta { get; set; } = new();

    BaseDelta IDeltaEntityRequest.Delta => Delta;

}
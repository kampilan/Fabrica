using Fabrica.Mediator;
using Fabrica.Mediator.Requests;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class UpdateEntityRequest<TEntity,TDelta> : IUpdateRequest, IRequest<Response<TEntity>> where TEntity : class, IModel where TDelta: BaseDelta, new()
{

    public string Uid { get; set; } = "";

    public TDelta Delta { get; set; } = new();

    BaseDelta IUpdateRequest.Delta => Delta;


}
using System.Collections.Generic;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class DeltaEntityRequest<TEntity>: IDeltaEntityRequest, IRequest<Response<TEntity>> where TEntity: class, IModel
{

    public OperationType Operation { get; set; }

    public string Uid { get; set; } = "";

    public IDictionary<string,object> Delta { get; set; } = new Dictionary<string, object>();


}
using System.Collections.Generic;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class UpdateEntityRequest<TEntity> : IRequest<Response<TEntity>> where TEntity : class, IModel
{

    public string Uid { get; set; } = "";

    public Dictionary<string, object> Properties { get; set; } = new();


}
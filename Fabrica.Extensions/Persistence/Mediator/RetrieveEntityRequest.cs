using Fabrica.Mediator;
using Fabrica.Mediator.Requests;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class RetrieveEntityRequest<TEntity> : IRetrieveRequest, IRequest<Response<TEntity>> where TEntity : class, IModel
{

    public string Uid { get; set; } = "";

}
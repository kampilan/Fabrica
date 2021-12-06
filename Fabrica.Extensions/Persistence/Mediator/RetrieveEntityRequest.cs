using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class RetrieveEntityRequest<TEntity> : BaseEntityRequest, IRetrieveEntityRequest, IRequest<Response<TEntity>> where TEntity : class, IModel
{

    public string Uid { get; set; } = "";

}
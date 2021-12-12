using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class RetrieveEntityRequest<TEntity> : BaseEntityRequest, IRetrieveEntityRequest, IRequest<Response<TEntity>> where TEntity : class, IModel
{

    public static RetrieveEntityRequest<TEntity> ForUid( string uid )
    {

        var request = new RetrieveEntityRequest<TEntity>
        {
            Uid = uid
        };

        return request;
    }


    public string Uid { get; set; } = "";

}
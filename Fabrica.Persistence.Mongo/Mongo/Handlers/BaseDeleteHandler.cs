using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using MediatR;
using MongoDB.Driver;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Mongo.Handlers;

public class BaseDeleteHandler<TRequest,TModel> : BaseHandler<TRequest> where TRequest : class, IRequest<Response>, IDeleteEntityRequest where TModel : class, IModel
{


    public BaseDeleteHandler(ICorrelation correlation, IMongoDbContext context ) : base(correlation)
    {

        Collection = context.GetCollection<TModel>();

    }

    private IMongoCollection<TModel> Collection { get; }


    protected override async Task Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to delete Entity using given Uid");
        await Collection.DeleteOneAsync(s => s.Uid == Request.Uid, cancellationToken);

    }


}
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using MediatR;
using MongoDB.Driver;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Mongo.Handlers;

public abstract class BaseRetrieveHandler<TRequest, TResponse> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>, IRetrieveEntityRequest where TResponse : class, IModel
{


    protected BaseRetrieveHandler(ICorrelation correlation, IMongoDbContext context ) : base(correlation)
    {

        Collection = context.GetCollection<TResponse>();

    }


    protected IMongoCollection<TResponse> Collection { get; }

    

    protected override async Task<TResponse> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to fetch one entity");
        var cursor = await Collection.FindAsync(e => e.Uid == Request.Uid, cancellationToken: cancellationToken);
        var entity = await cursor.SingleOrDefaultAsync(cancellationToken);

        if( entity is null )
            throw new NotFoundException($"Could not find {typeof(TResponse).Name} using Uid = ({Request.Uid})");

        logger.LogObject(nameof(entity), entity);



        // *****************************************************************
        return entity;

    }


}

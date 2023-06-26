using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using MongoDB.Driver;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Mongo.Mediator.Handlers;

public abstract class BaseRetrieveHandler<TRequest, TResponse> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>, IRetrieveEntityRequest where TResponse : class, IModel
{


    protected BaseRetrieveHandler(ICorrelation correlation, IMongoDbContext context) : base(correlation)
    {

        Context    = context;
        Collection = context.GetCollection<TResponse>();

    }

    protected IMongoDbContext Context { get;  }
    private IMongoCollection<TResponse> Collection { get; }


    protected virtual Task Populate( TResponse parent )
    {
        return Task.CompletedTask;
    }


    protected override async Task<TResponse> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to fetch one entity");
        var cursor = await Collection.FindAsync(e => e.Uid == Request.Uid, cancellationToken: cancellationToken);
        var entity = await cursor.SingleOrDefaultAsync(cancellationToken);

        if (entity is null)
            throw new NotFoundException($"Could not find {typeof(TResponse).Name} using Uid = ({Request.Uid})");

        logger.LogObject(nameof(entity), entity);



        // *****************************************************************
        logger.Debug("Attempting to fetch details");
        await Populate(entity);



        // *****************************************************************
        logger.Debug("Attempting to post the entity");
        if (entity is IMutableModel mu)
            mu.Post();



        // *****************************************************************
        return entity;

    }


}

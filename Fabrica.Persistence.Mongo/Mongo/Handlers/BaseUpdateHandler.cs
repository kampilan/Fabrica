using AutoMapper;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using MediatR;
using MongoDB.Driver;
using static System.Formats.Asn1.AsnWriter;

namespace Fabrica.Persistence.Mongo.Handlers;

public class BaseUpdateHandler<TRequest, TResponse> : BaseHandler<TRequest, TResponse> where TRequest : class, IUpdateEntityRequest, IRequest<Response<TResponse>> where TResponse : class, IModel
{

    
    public BaseUpdateHandler(ICorrelation correlation, IMongoDbContext context, IMapper mapper ) : base(correlation)
    {

        Collection = context.GetCollection<TResponse>();
        Mapper = mapper;


    }

    private IMongoCollection<TResponse> Collection { get; }
    private IMapper Mapper { get; }


    protected override async Task<TResponse> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to Find WatchDomain for update");
        var cursor = await Collection.FindAsync(e => e.Uid == Request.Uid, cancellationToken: cancellationToken);
        var entity = await cursor.SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (entity is null)
            throw new NotFoundException($"Could not find {typeof(TResponse).FullName} for update using Uid = ({Request.Uid})");



        // *****************************************************************
        logger.Debug("Attempting to map Delta to WatchDomain");
        Mapper.Map(Request.Delta, entity);

        logger.LogObject(nameof(entity), entity);



        // *****************************************************************
        logger.Debug("Attempting to update WatchDomain");

        var options = new ReplaceOptions
        {
            IsUpsert = true
        };

        await Collection.ReplaceOneAsync( e => e.Uid == entity.Uid, entity, options, cancellationToken: cancellationToken );



        // *****************************************************************
        return entity;


    }

}
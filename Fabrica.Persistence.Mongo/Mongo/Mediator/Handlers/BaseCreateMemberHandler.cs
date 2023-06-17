using AutoMapper;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using MediatR;
using MongoDB.Driver;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Mongo.Mediator.Handlers;
public abstract class BaseCreateMemberHandler<TRequest, TParent, TChild> : BaseHandler<TRequest, TChild> where TRequest : class, ICreateMemberEntityRequest, IRequest<Response<TChild>> where TParent: class, IModel where TChild : class, IModel, new()
{

    protected BaseCreateMemberHandler(ICorrelation correlation, IMongoDbContext context, IMapper mapper) : base(correlation)
    {
    
        ParentCollection = context.GetCollection<TParent>();
        ChildCollection = context.GetCollection<TChild>();
        Mapper = mapper;

    }

    private IMongoCollection<TParent> ParentCollection { get; }
    private IMongoCollection<TChild> ChildCollection { get; }
    private IMapper Mapper { get; }
    protected abstract Action<TParent, TChild> Attach { get; }


    protected virtual TChild CreateEntity()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.DebugFormat("Attempting to create new {0}", typeof(TChild).FullName ?? "");
        var entity = new TChild();


        // *****************************************************************
        return entity;

    }


    protected override async Task<TChild> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to fetch parent using ParentUid");

        var cursor = await ParentCollection.FindAsync(p => p.Uid == Request.ParentUid, cancellationToken: cancellationToken);
        var parent = await cursor.SingleOrDefaultAsync(cancellationToken: cancellationToken);
        if (parent is null)
            throw new NotFoundException($"Could not find Parent ({typeof(TParent).Name}) using ParentUid = ({Request.ParentUid}) for Child ({typeof(TChild)})");

        logger.LogObject(nameof(parent), parent);



        // *****************************************************************
        logger.Debug("Attempting to create new entity ");
        var child = CreateEntity();

        if( !string.IsNullOrWhiteSpace(Request.Uid) )
            child.Uid = Request.Uid;


        // *****************************************************************
        logger.Debug("Attempting to Attach Child to Parent");
        Attach(parent, child);



        // *****************************************************************
        logger.Debug("Attempting to map Delta to to new entity");
        Mapper.Map(Request.Delta, child);

        logger.LogObject(nameof(child), child);



        // *****************************************************************
        logger.Debug("Attempting to Insert Entity in collection");
        await ChildCollection.InsertOneAsync(child, cancellationToken: cancellationToken);



        // *****************************************************************
        return child;

    }


}
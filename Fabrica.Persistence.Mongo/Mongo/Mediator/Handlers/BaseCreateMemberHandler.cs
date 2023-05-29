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

public abstract class BaseCreateMemberHandler<TRequest, TParent, TChild>: BaseCreateHandler<TRequest,TChild> where TRequest : class, ICreateMemberEntityRequest, ICreateEntityRequest, IRequest<Response<TChild>> where TParent : class, IModel where TChild : class, IModel, new()
{
    
    protected BaseCreateMemberHandler(ICorrelation correlation, IMongoDbContext context, IMapper mapper) : base(correlation, context, mapper)
    {

        Collection = context.GetCollection<TParent>();

    }


    private IMongoCollection<TParent> Collection { get; }

    protected abstract Action<TParent, TChild> Attach { get; }


    protected override async Task<TChild> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to fetch parent using ParentUid");

        var cursor = await Collection.FindAsync(p => p.Uid == Request.ParentUid, cancellationToken: cancellationToken);
        var parent = await cursor.SingleOrDefaultAsync(cancellationToken: cancellationToken);
        if (parent is null)
            throw new NotFoundException($"Could not find Parent ({typeof(TParent).Name}) using ParentUid = ({Request.ParentUid}) for Child ({typeof(TChild)})");

        logger.LogObject(nameof(parent), parent);



        // *****************************************************************
        logger.Debug("Attempting to call base Perform");
        var child = await base.Perform(cancellationToken);



        // *****************************************************************
        logger.Debug("Attempting to Attach Child to Parent");
        Attach(parent, child);


        // *****************************************************************
        return child;

    }


}
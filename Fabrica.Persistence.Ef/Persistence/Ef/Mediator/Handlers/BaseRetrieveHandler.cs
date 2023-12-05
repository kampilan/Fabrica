using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Mediator.Handlers;

public abstract class BaseRetrieveHandler<TRequest, TResponse, TDbContext> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>, IRetrieveEntityRequest where TResponse : class, IModel where TDbContext : DbContext, IOriginDbContext
{


    protected BaseRetrieveHandler( ICorrelation correlation, TDbContext context) : base(correlation)
    {

        Context = context;

    }


    protected TDbContext Context { get; }


    protected abstract Func<TDbContext, IQueryable<TResponse>> One { get; }


    protected override async Task<TResponse> Perform( CancellationToken cancellationToken=default )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to fetch one entity");
        var entity = await One(Context).SingleOrDefaultAsync(e => e.Uid == Request.Uid, cancellationToken: cancellationToken);

        if (entity is null)
            throw new NotFoundException($"Could not find {typeof(TResponse).Name} using Uid = ({Request.Uid})");

        logger.LogObject(nameof(entity), entity);



        // *****************************************************************
        return entity;

    }


}
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Mediator.Handlers
{

    
    public abstract class BaseDeleteHandler<TRequest,TModel,TDbContext> : BaseHandler<TRequest> where TRequest : class, IRequest<Response>, IDeleteEntityRequest where TModel: class, IModel where TDbContext : DbContext, IOriginDbContext
    {


        protected BaseDeleteHandler( ICorrelation correlation, IUnitOfWork uow, TDbContext context ): base(correlation)
        {

            Uow     = uow;
            Context = context;

        }

        protected IUnitOfWork Uow { get; }
        protected TDbContext Context { get; }


        protected abstract Func<TDbContext,IQueryable<TModel>> One { get; }

        protected TModel Entity { get; private set; } = null!;


        protected override async Task Before()
        {

            using var logger = EnterMethod();
            
            await  base.Before();


            // *****************************************************************
            logger.Debug("Attempting to fetch one entity");
            var entity = await One(Context).SingleOrDefaultAsync(e => e.Uid == Request.Uid);

            if (entity is null)
                throw new NotFoundException($"Could not find {typeof(TModel).Name} using Uid ({Request.Uid})");

            logger.LogObject(nameof(entity), entity);


            Entity = entity;


        }

        protected override Task Perform(CancellationToken cancellationToken = default)
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to remove entity");
            Context.Remove(Entity);


            return Task.CompletedTask;

        }


        protected override async Task InternalSuccess()
        {

            await Context.SaveChangesAsync();

        }


    }


}

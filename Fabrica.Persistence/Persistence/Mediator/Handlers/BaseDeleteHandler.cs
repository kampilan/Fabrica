using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Mediator.Requests;
using Fabrica.Models.Support;
using Fabrica.Persistence.Contexts;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public abstract class BaseDeleteHandler<TRequest,TModel,TDbContext> : BaseHandler<TRequest> where TRequest : class, IRequest<Response>, IDeleteRequest where TModel: class, IModel where TDbContext: OriginDbContext
    {


        protected BaseDeleteHandler( ICorrelation correlation, IUnitOfWork uow, TDbContext context ): base(correlation)
        {

            Uow     = uow;
            Context = context;

        }

        protected IUnitOfWork Uow { get; }
        protected TDbContext Context { get; }

        protected virtual IQueryable<TModel> GetQueryable()
        {

            using var logger = EnterMethod();

            return Context.Set<TModel>().AsQueryable();


        }

        protected override async Task Perform(CancellationToken cancellationToken = default)
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to fetch one entity by uid");
            var entity = await GetQueryable().SingleOrDefaultAsync( e => e.Uid == Request.Uid, cancellationToken: cancellationToken );

            if (entity is null)
                throw new NotFoundException($"Could not find {typeof(TModel).Name} using Uid = ({Request.Uid})");



            // *****************************************************************
            logger.Debug("Attempting to remove entity");
            Context.Remove(entity);


        }


        protected override async Task<Response> Success( TRequest request )
        {

            await Context.SaveChangesAsync();
            await base.Success(request);

            return new Response().IsOk();

        }


    }


}

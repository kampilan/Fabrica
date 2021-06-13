using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Mediator.Requests;
using Fabrica.Models.Support;
using Fabrica.Persistence.Contexts;
using Fabrica.Utilities.Container;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public abstract class BaseRetrieveHandler<TRequest, TResponse, TDbContext> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>, IRetrieveRequest where TResponse : class, IModel where TDbContext : OriginDbContext
    {


        protected BaseRetrieveHandler( ICorrelation correlation, TDbContext context) : base(correlation)
        {

            Context = context;

        }


        protected TDbContext Context { get; }


        protected virtual IQueryable<TResponse> GetQueryable()
        {

            using var logger = EnterMethod();

            return Context.Set<TResponse>().AsQueryable();

        }

        protected override async Task<TResponse> Perform( CancellationToken cancellationToken=default )
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to fetch one entity");
            var entity = await GetQueryable().SingleOrDefaultAsync(e => e.Uid == Request.Uid, cancellationToken: cancellationToken);

            if (entity is null)
                throw new NotFoundException($"Could not find {typeof(TResponse).Name} using Uid = ({Request.Uid})");

            logger.LogObject(nameof(entity), entity);



            // *****************************************************************
            return entity;

        }


    }

}

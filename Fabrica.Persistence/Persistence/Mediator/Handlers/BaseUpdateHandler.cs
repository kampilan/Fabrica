using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

    public abstract class BaseUpdateHandler<TRequest,TResponse,TDbContext>: BaseMutableHandler<TRequest,TResponse,TDbContext> where TRequest: class, IUpdateRequest, IRequest<Response<TResponse>> where TResponse: class, IModel where TDbContext: OriginDbContext
    {

        protected BaseUpdateHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, TDbContext context, IMapper mapper ): base( correlation, meta, uow, context, mapper)
        {
        }


        protected virtual IQueryable<TResponse> GetQueryable()
        {

            using var logger = EnterMethod();

            return Context.Set<TResponse>().AsQueryable();


        }


        protected override async Task<TResponse> GetEntity()
        {

            using var logger = EnterMethod();

            var entity = await GetQueryable().SingleOrDefaultAsync(e => e.Uid == Request.Uid);

            if( entity is null )
                throw new NotFoundException($"Could not find {typeof(TResponse)} using Uid ({Request.Uid})");


            return entity;

        }


    }

}

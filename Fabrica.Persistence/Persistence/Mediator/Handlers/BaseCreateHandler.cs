using System.Threading.Tasks;
using AutoMapper;
using Fabrica.Mediator;
using Fabrica.Mediator.Requests;
using Fabrica.Models.Support;
using Fabrica.Persistence.Contexts;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public abstract class BaseCreateHandler<TRequest,TResponse,TDbContext> : BaseMutableHandler<TRequest, TResponse, TDbContext> where TRequest : class, ICreateRequest, IRequest<Response<TResponse>> where TResponse : class, IModel, new() where TDbContext: OriginDbContext
    {


        protected BaseCreateHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, TDbContext context, IMapper mapper): base( correlation, meta, uow, context, mapper )
        {
        }

        protected override Task<TResponse> GetEntity()
        {
            return Task.FromResult(new TResponse());
        }


    }

}

using System.Threading.Tasks;
using AutoMapper;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Contexts;
using Fabrica.Persistence.Mediator.Requests;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public abstract class BaseCreateHandler<TRequest, TResponse,TDbContext> : BaseMutableHandler<TRequest, TResponse, TDbContext> where TRequest : class, IMutableRequest, IRequest<Response<TResponse>> where TResponse : class, IModel, new() where TDbContext: OriginDbContext
    {


        protected BaseCreateHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, TDbContext context, IMapper mapper): base( correlation, meta, uow, context, mapper )
        {
        }

        protected override Task<TResponse> GetEntity()
        {
            return Task.FromResult(new TResponse());
        }


        protected override void Validate()
        {

            var overposted = Meta.CheckForCreate(Request.Properties.Keys);
            if (overposted.Count > 0)
                throw new PredicateException($"The following properties were not found or are not mutable: ({string.Join(',', overposted)})").WithErrorCode("DisallowedProperties");

        }


    }

}

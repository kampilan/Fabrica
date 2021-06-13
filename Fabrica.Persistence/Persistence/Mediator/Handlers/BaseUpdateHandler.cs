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

    public abstract class BaseUpdateHandler<TRequest,TResponse>: BaseMutableHandler<TRequest,TResponse> where TRequest: class, IMutableRequest, IRequest<Response<TResponse>> where TResponse: class, IModel
    {

        protected BaseUpdateHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, OriginDbContext context, IMapper mapper ): base( correlation, meta, uow, context, mapper)
        {
        }

        protected override void Validate()
        {

            var overposted = Meta.CheckForUpdate(Request.Properties.Keys);
            if( overposted.Count > 0 )
                throw new PredicateException($"The following properties were not found or are not mutable: ({string.Join(',', overposted)})").WithErrorCode("DisallowedProperties");

        }


    }

}

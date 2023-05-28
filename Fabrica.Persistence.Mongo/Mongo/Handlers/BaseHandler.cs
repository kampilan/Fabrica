using Fabrica.Mediator;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.Persistence.Mongo.Handlers;


public abstract class BaseHandler<TRequest, TResponse> : AbstractRequestHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>
{

    protected BaseHandler(ICorrelation correlation) : base(correlation)
    {

    }

}


public abstract class BaseHandler<TRequest> : AbstractRequestHandler<TRequest> where TRequest : class, IRequest<Response>
{

    protected BaseHandler(ICorrelation correlation) : base(correlation)
    {

    }

}


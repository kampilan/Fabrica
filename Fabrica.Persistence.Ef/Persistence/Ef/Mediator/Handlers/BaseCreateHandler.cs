using AutoMapper;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Mediator.Handlers;

public abstract class BaseCreateHandler<TRequest, TResponse, TDbContext> : BaseDeltaHandler<TRequest, TResponse, TDbContext> where TRequest : class, IRequest<Response<TResponse>>, ICreateEntityRequest where TResponse : class, IModel, new() where TDbContext : DbContext, IOriginDbContext
{
    
    protected BaseCreateHandler(ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, TDbContext context, IMapper mapper) : base(correlation, meta, uow, context, mapper)
    {
    }

    protected override OperationType Operation => OperationType.Create;

}
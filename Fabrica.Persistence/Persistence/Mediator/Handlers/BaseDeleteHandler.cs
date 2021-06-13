using System;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Persistence.Contexts;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public abstract class BaseDeleteHandler<TRequest> : BaseHandler<TRequest> where TRequest : class, IRequest<Response>
    {


        protected BaseDeleteHandler( ICorrelation correlation, IUnitOfWork uow, OriginDbContext context ): base(correlation)
        {

            Uow     = uow;
            Context = context;

        }

        protected IUnitOfWork Uow { get; }
        protected OriginDbContext Context { get; }


        protected override async Task<Response> Success( TRequest request )
        {

            await Context.SaveChangesAsync();
            await base.Success(request);

            return new Response().IsOk();

        }


    }


}

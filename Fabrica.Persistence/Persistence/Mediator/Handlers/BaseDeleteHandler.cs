using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Persistence.Contexts;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public abstract class BaseDeleteHandler<TRequest,TDbContext> : BaseHandler<TRequest> where TRequest : class, IRequest<Response> where TDbContext: OriginDbContext
    {


        protected BaseDeleteHandler( ICorrelation correlation, IUnitOfWork uow, TDbContext context ): base(correlation)
        {

            Uow     = uow;
            Context = context;

        }

        protected IUnitOfWork Uow { get; }
        protected TDbContext Context { get; }


        protected override async Task<Response> Success( TRequest request )
        {

            await Context.SaveChangesAsync();
            await base.Success(request);

            return new Response().IsOk();

        }


    }


}

using System;
using System.Linq;
using System.Threading;
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


    public abstract class BaseMemberCreateHandler<TRequest,TParent,TChild,TDbContext> : BaseDeltaHandler<TRequest, TChild, TDbContext> where TRequest : class, IMemberCreateRequest, IRequest<Response<TChild>> where TParent : class, IModel where TChild : class, IModel, new() where TDbContext : OriginDbContext
    {


        protected BaseMemberCreateHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, TDbContext context, IMapper mapper ) : base( correlation, meta, uow, context, mapper )
        {

        }


        protected override Task<TChild> CreateEntity()
        {
            return Task.FromResult( new TChild() );
        }

        protected sealed override Func<TDbContext,IQueryable<TChild>> One => c => c.Set<TChild>().AsQueryable();
        protected abstract Func<TDbContext,IQueryable<TParent>> OneParent { get; }
        protected abstract Action<TParent,TChild> Attach { get; }


        protected override async Task<TChild> Perform(CancellationToken cancellationToken = default)
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to fetch parent using ParentUid");
            var parent = await OneParent(Context).SingleOrDefaultAsync(e => e.Uid == Request.ParentUid, cancellationToken: cancellationToken);
            if (parent is null)
                throw new NotFoundException( $"Could not find Parent ({typeof(TParent).Name}) using ParentUid = ({Request.ParentUid}) for Child ({typeof(TChild)})" );

            logger.LogObject(nameof(parent), parent);



            // *****************************************************************
            logger.Debug("Attempting to call base Perform");
            var child = await base.Perform(cancellationToken);



            // *****************************************************************
            logger.Debug("Attempting to Attach Child to Parent");
            Attach( parent, child );


            // *****************************************************************
            return child;


        }


    }


}

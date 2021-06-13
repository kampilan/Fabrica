using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Contexts;
using Fabrica.Persistence.Mediator.Requests;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers
{
 
    
    public abstract class BaseMutableHandler<TRequest, TResponse> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>, IMutableRequest where TResponse : class, IModel
    {


        protected BaseMutableHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, OriginDbContext context, IMapper mapper): base(correlation)
        {

            Meta    = meta.GetMetaFromType(typeof(TResponse));
            Uow     = uow;
            Context = context;
            Mapper  = mapper;

        }

        protected ModelMeta Meta { get; }

        protected IUnitOfWork Uow { get; }

        protected OriginDbContext Context { get; }
        protected IMapper Mapper { get; }

        protected abstract void Validate();


        protected async Task ApplyReference<TReference>( [NotNull] TResponse target, [NotNull] Expression<Func<TResponse,TReference>> getter, CancellationToken token=default ) where TReference: class, IReferenceModel
        {

            if (target == null) throw new ArgumentNullException(nameof(target));
            if (getter == null) throw new ArgumentNullException(nameof(getter));


            using var logger = EnterMethod();

            if( getter.Body is MemberExpression {Member: PropertyInfo {CanWrite: true} pi} && Request.Properties.TryGetValue(pi.Name, out var value ) )
            {

                var uid = value.ToString();

                logger.Inspect("Target Type", typeof(TResponse).FullName);
                logger.Inspect("Reference Type", typeof(TReference).FullName);
                logger.Inspect(nameof(pi.Name), pi.Name);
                logger.Inspect(nameof(uid), uid);



                // *****************************************************************
                logger.Debug("Attempting to fetch reference using uid");
                var re = await Context.Set<TReference>().SingleOrDefaultAsync(e => e.Uid == uid, cancellationToken: token);
                if (re == null)
                    throw new NotFoundException($"Could not find {typeof(TReference).Name} for Property ({pi.Name}) using ({uid})");



                // *****************************************************************
                logger.Debug("Attempting to set property on target");
                pi.GetSetMethod()?.Invoke(target, new object[]{re});



                // *****************************************************************
                logger.Debug("Attempting to remove value from properties");
                Request.Properties.Remove(pi.Name);


            }


        }

        protected void Apply( [NotNull] TResponse target )
        {

            if (target == null) throw new ArgumentNullException(nameof(target));

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to validate properties");
            Validate();



            // *****************************************************************
            logger.Debug("Attempting to map properties to entity" );
            Mapper.Map( Request.Properties, target);


        }


        protected override async Task<TResponse> Success(TRequest request, TResponse response)
        {

            using var logger = EnterMethod();
            
            await Context.SaveChangesAsync();

            Uow.CanCommit();

            return await base.Success(request, response);

        }


        protected override void Failure(TRequest request, Exception cause)
        {

            using var logger = EnterMethod();

            Uow.MustRollback();

        }


    }

}

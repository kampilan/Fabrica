using System;
using System.Linq.Expressions;
using System.Reflection;
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
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers
{


    public abstract class BaseMutableHandler<TRequest, TResponse, TDbContext> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>, IMutableRequest where TResponse : class, IModel where TDbContext : OriginDbContext
    {


        protected BaseMutableHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, TDbContext context, IMapper mapper ) : base( correlation )
        {

            Meta = meta.GetMetaFromType(typeof(TResponse));
            Uow = uow;
            Context = context;
            Mapper = mapper;

        }

        protected ModelMeta Meta { get; }

        protected IUnitOfWork Uow { get; }

        protected TDbContext Context { get; }
        protected IMapper Mapper { get; }


        protected DeltaPropertySet Properties { get; private set; }


        protected abstract Task<TResponse> GetEntity();


        protected async Task ApplyReference<TReference>([NotNull] TResponse target, [NotNull] Expression<Func<TResponse, TReference>> getter, CancellationToken token = default) where TReference : class, IModel
        {

            if (target == null) throw new ArgumentNullException(nameof(target));
            if (getter == null) throw new ArgumentNullException(nameof(getter));


            using var logger = EnterMethod();


            if (getter.Body is MemberExpression { Member: PropertyInfo { CanWrite: true } pi } && Properties.TryGetValue(pi.Name, out var value))
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
                pi.GetSetMethod()?.Invoke(target, new object[] { re });



                // *****************************************************************
                logger.Debug("Attempting to remove value from properties");
                Properties.Remove(pi.Name);


            }


        }

        protected async Task ApplyReferenceByName<TReference>([NotNull] TResponse target, [NotNull] string name, CancellationToken token = default) where TReference : class, IModel
        {

            if (target == null) throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));


            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to get value from properties");
            var uid = "";
            if( Properties.TryGetValue(name, out var value) )
                uid = value.ToString();

            if (string.IsNullOrWhiteSpace(uid))
                return;



            logger.Inspect("typeof(TReference)", typeof(TReference).FullName);
            logger.Inspect(nameof(name), name);
            logger.Inspect(nameof(uid), uid);



            // *****************************************************************
            logger.Debug("Attempting to find writable property on target");
            var pi = target.GetType().GetProperty(name);
            if (pi is null)
                throw new Exception($"Could not find property ({name}) on Type {typeof(TResponse)}");

            if (!pi.CanWrite)
                throw new Exception($"Can write property ({name}) on Type {typeof(TResponse)}");



            // *****************************************************************
            logger.Debug("Attempting to dig out setter method");
            var method = pi.GetSetMethod();
            if (method is null)
                throw new Exception($"Could not get Setter for property ({name}) on Type {typeof(TResponse)}");



            // *****************************************************************
            logger.Debug("Attempting to fetch reference using uid");
            var replacement = await Context.Set<TReference>().SingleOrDefaultAsync(e => e.Uid == uid, cancellationToken: token);
            if (replacement == null)
                throw new NotFoundException($"Could not find {typeof(TReference).Name} for Property ({name}) using ({uid})");

            logger.LogObject(nameof(replacement), replacement);



            // *****************************************************************
            logger.Debug("Attempting to call setter with new reference");
            method.Invoke(target, new object[] { replacement });



            // *****************************************************************
            logger.Debug("Attempting to remove value form properties");
            Properties.Remove(name);


        }

        protected void Apply([NotNull] TResponse target)
        {

            if (target == null) throw new ArgumentNullException(nameof(target));

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to validate properties");
            Validate();



            // *****************************************************************
            logger.Debug("Attempting to map properties to entity");
            Mapper.Map( Properties, target );


        }

        protected override async Task<TResponse> Perform( CancellationToken cancellationToken = default )
        {


            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to get target");
            var target = await GetEntity();

            logger.LogObject(nameof(target), target);



            // *****************************************************************
            logger.Debug("Attempting to get property set from delta");
            Properties = Request.Delta?.GetPropertySet() ?? new DeltaPropertySet();
            logger.LogObject(nameof(Properties), Properties);



            // *****************************************************************
            logger.Debug("Attempting to check for references");
            foreach( var re in Context.Entry(target).References )
            {

                if( Properties.ContainsKey( re.Metadata.PropertyInfo.Name ) )
                {
                    var call = GetType().GetMethod("ApplyReferenceByName", (BindingFlags.Instance | BindingFlags.NonPublic))?.MakeGenericMethod(re.Metadata.PropertyInfo.PropertyType);
                    if (call?.Invoke(this, new object[] {target, re.Metadata.PropertyInfo.Name, cancellationToken}) is Task task)
                        await task;
                }

            }



            // *****************************************************************
            logger.Debug("Attempting to validate properties");
            await Validate();



            // *****************************************************************
            logger.Debug("Attempting to apply properties");
            Apply( target );



            // *****************************************************************
            logger.Debug("Attempting to check for detached state");

            if( Context.Entry(target).State == EntityState.Detached )
                await Context.AddAsync( target, cancellationToken );



            // *****************************************************************
            return target;

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

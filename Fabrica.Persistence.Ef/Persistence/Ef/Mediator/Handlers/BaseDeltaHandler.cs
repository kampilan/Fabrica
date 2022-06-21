using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Mediator.Handlers
{


    public abstract class BaseDeltaHandler<TRequest, TResponse, TDbContext> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>, IDeltaEntityRequest where TResponse : class, IModel, new() where TDbContext : OriginDbContext
    {


        protected BaseDeltaHandler( ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, TDbContext context, IMapper mapper ) : base( correlation )
        {

            Meta    = meta.GetMetaFromType(typeof(TResponse));
            Uow     = uow;
            Context = context;
            Mapper  = mapper;

        }

        protected ModelMeta Meta { get; }

        protected IUnitOfWork Uow { get; }

        protected TDbContext Context { get; }
        protected IMapper Mapper { get; }

        protected abstract OperationType Operation { get; }
        protected abstract Func<TDbContext,IQueryable<TResponse>> One { get; }


        protected TResponse Entity { get; private set; }



        protected virtual Task<TResponse> CreateEntity()
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to create new entity");
            var entity = new TResponse();

            logger.LogObject(nameof(entity), entity);



            // *****************************************************************
            return Task.FromResult( entity );

        }

        protected virtual async Task<TResponse> RetrieveEntity()
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to fetch one entity");
            var entity = await One(Context).SingleOrDefaultAsync(e => e.Uid == Request.Uid);

            if (entity is null)
                throw new NotFoundException($"Could not find {typeof(TResponse).Name} using Uid ({Request.Uid})");

            logger.LogObject(nameof(entity), entity);



            // *****************************************************************
            return entity;

        }


        private IList<DuplicateCheckBuilder<TResponse>> DuplicateChecks { get; } = new List<DuplicateCheckBuilder<TResponse>>();

        public void AddDuplicateCheck( Func<TResponse,TResponse, string> template, Func<TResponse, Expression<Func<TResponse, bool>>> predicate )
        {

            using var logger = EnterMethod();
            
            DuplicateChecks.Add( new DuplicateCheckBuilder<TResponse>( template, predicate) );

        }

        protected async Task CheckForDuplicates()
        {


            using var logger = EnterMethod();


            logger.Inspect(nameof(DuplicateChecks.Count), DuplicateChecks.Count);


            PredicateException pe = null;

            foreach (var check in DuplicateChecks )
            {


                var (checker, template) = check.Build(Entity);

                var exists = await Context.Set<TResponse>().FirstOrDefaultAsync(checker);
                if( exists == null )
                    continue;


                pe ??= new PredicateException("Duplicate found");


                var message = template( Entity, exists );

                pe.WithDetail(new EventDetail { Group = $"{typeof(TResponse).Name}.Duplicates", Explanation = message });


            }

            if (pe != null)
                throw pe;

        }



        protected IDictionary<string,object> Properties { get; private set; }


        private NullabilityInfoContext _nulctx = new NullabilityInfoContext();
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
                TReference re;
                if (!string.IsNullOrWhiteSpace(uid))
                {
                    re = await Context.Set<TReference>().SingleOrDefaultAsync(e => e.Uid == uid, cancellationToken: token);
                    if (re == null)
                        throw new NotFoundException($"Could not find {typeof(TReference).Name} for Property ({pi.Name}) using ({uid})");
                }
                else if (_nulctx.Create(pi).WriteState is NullabilityState.Nullable)
                    re = null;
                else
                    throw new ValidationException(new List<EventDetail>{new ()
                    {
                        Category    = EventDetail.EventCategory.Violation,
                        Group       = $"{typeof(TResponse).Name}.{pi.Name}",
                        Explanation = $"{pi.Name} on {typeof(TResponse).Name} is not optional.",
                        Source      = target.ToString(),
                        RuleName    = "NA"
                    } });



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

            TReference replacement;
            if( !string.IsNullOrWhiteSpace(uid) )
            {
                
                replacement = await Context.Set<TReference>().SingleOrDefaultAsync(e => e.Uid == uid, cancellationToken: token);
                if (replacement == null)
                    throw new NotFoundException($"Could not find {typeof(TReference).Name} for Property ({name}) using ({uid})");

                logger.LogObject(nameof(replacement), replacement);

            }
            else if( _nulctx.Create(pi).WriteState is NullabilityState.Nullable )
                replacement = null;
            else
                throw new ValidationException( new List<EventDetail>{new ()
                {
                    Category    = EventDetail.EventCategory.Violation,
                    Group       = $"{typeof(TResponse).Name}.{pi.Name}",
                    Explanation = $"{pi.Name} on {typeof(TResponse).Name} is not optional.",
                    Source      = target.ToString(),
                    RuleName    = "NA"
                } });



            // *****************************************************************
            logger.Debug("Attempting to call setter with new reference");
            method.Invoke(target, new object[] { replacement });



            // *****************************************************************
            logger.Debug("Attempting to remove value form properties");
            Properties.Remove(name);


        }


        protected override async Task Before()
        {

            using var logger = EnterMethod();

            await base.Before();


            // *****************************************************************
            if( Operation == OperationType.Create )
            {
                logger.Debug("Attempting to create entity");
                Entity = await CreateEntity();
            }
            else if (Operation == OperationType.Update)
            {
                logger.Debug("Attempting to create entity");
                Entity = await RetrieveEntity();
            }
            else
                throw new InvalidOperationException( $"Invalid Operation Type: ({Operation}) for Delta Handler: {GetType().FullName}" );

        }


        protected override async Task<TResponse> Perform( CancellationToken cancellationToken = default )
        {


            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to get property set from delta");
            Properties = Request.Delta ?? new Dictionary<string, object>();
            logger.LogObject(nameof(Properties), Properties);



            // *****************************************************************
            logger.Debug("Attempting to check for references");
            foreach( var re in Context.Entry(Entity).References )
            {

                if( re.Metadata.PropertyInfo is null || !Properties.ContainsKey(re.Metadata.PropertyInfo.Name) )
                    continue;

                var call = GetType().GetMethod("ApplyReferenceByName", (BindingFlags.Instance | BindingFlags.NonPublic))?.MakeGenericMethod(re.Metadata.PropertyInfo.PropertyType);
                if (call?.Invoke(this, new object[] {Entity, re.Metadata.PropertyInfo.Name, cancellationToken}) is Task task)
                    await task;

            }



            // *****************************************************************
            logger.Debug("Attempting to map properties to entity");
            Mapper.Map(Properties, Entity);



            // *****************************************************************
            logger.Debug("Attempting to perform duplicate checks");
            await CheckForDuplicates();



            // *****************************************************************
            logger.Debug("Attempting to check for detached state");
            if( Operation == OperationType.Create )
                await Context.AddAsync( Entity, cancellationToken );



            // *****************************************************************
            return Entity;

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

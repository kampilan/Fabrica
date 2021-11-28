using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fabrica.Exceptions;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.Persistence.Patch
{

    public class PatchResolverComponent: IPatchResolverComponent
    {


        public PatchResolverComponent( IModelMetaService meta, IPatchRepository repository, IMapper mapper )
        {
            Meta       = meta;
            Repository = repository;
            Mapper     = mapper;
        }


        private IModelMetaService Meta { get; }
        private IPatchRepository Repository { get; }
        private IMapper Mapper { get; }


        protected virtual ILogger GetLogger()
        {
            return WatchFactoryLocator.Factory.GetLogger(GetType());
        }


        protected virtual async Task ApplyProperties([NotNull]object target, [NotNull]IDictionary<string, object> properties)
        {

            if (target == null) throw new ArgumentNullException(nameof(target));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect("Target Type", target.GetType().FullName);



                // *****************************************************************
                logger.Debug("Attempting to handle References");
                var references = properties.Where(p => p.Key.Contains(":") && p.Value is string).ToList();

                logger.Inspect( nameof(references.Count), references.Count );

                foreach( var pair in references )
                {

                    logger.Inspect( nameof(pair.Key), pair.Key );
                    logger.Inspect( nameof(pair.Value), pair.Value);

                    var segs = pair.Key.Split(':');
                    if( segs.Length == 2 )
                    {

                        var resource     = segs[0];
                        var propertyName = segs[1];
                        var uid          = pair.Value.ToString();

                        object reference = null;

                        if( !string.IsNullOrWhiteSpace(uid) )
                        {
                            var meta = Meta.GetMetaFromAlias(resource);

                            logger.Debug("Attempting to fetch reference");
                            reference = await Repository.HandleRetrieve( meta.Target, pair.Value.ToString() );
                            logger.LogObject(nameof(reference), reference);
                        }

                        properties.Remove(pair.Key);
                        properties[propertyName] = reference;

                    }


                }



                // *****************************************************************
                logger.Debug("Attempting to map properties to target");
                Mapper.Map(properties, target);


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual Task UpdateAggregate( [NotNull] object parent, [NotNull] string propertyName, PatchVerb state, [NotNull] object aggregate )
        {

            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (aggregate == null) throw new ArgumentNullException(nameof(aggregate));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect("Parent Type", parent.GetType().FullName);
                logger.Inspect(nameof(propertyName), propertyName);
                logger.Inspect(nameof(state), state.ToString());
                logger.Inspect("Aggregate Type", aggregate.GetType().FullName);


                // *****************************************************************
                logger.Debug("Attempting to get property");
                var prop = parent.GetType().GetProperty(propertyName);
                if (prop != null)
                {

                    logger.Debug("Attempting to invoke property getter");
                    var collection = prop.GetMethod?.Invoke(parent, Array.Empty<object>());

                    logger.Inspect(nameof(collection), collection);

                    switch( collection )
                    {
                        case null:
                            return Task.CompletedTask;

                        case IAggregateCollection ac when aggregate is IModel model:
                            switch (state)
                            {

                                case PatchVerb.Create:

                                    ac.AddMember(model);
                                    break;

                                case PatchVerb.Delete:

                                    ac.RemoveMember(model);
                                    break;

                                case PatchVerb.Unmodified:
                                case PatchVerb.Update:
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException(nameof(state), state, null);

                            }

                            break;
                        default:
                            logger.WarningFormat( "Encountered Collection that does not support IAggregateCollection on Model: ({0}) Property: ({1})", parent.GetType().FullName, propertyName );
                            break;

                    }


                }


                return Task.CompletedTask;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        protected virtual Task BeforeApply()
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.CompletedTask;

            }
            finally
            {
                logger.LeaveMethod();
            }
           

        }


        public async Task Apply( [NotNull] PatchSet patchSet, bool save=true )
        {

            if (patchSet == null) throw new ArgumentNullException(nameof(patchSet));

            var logger = GetLogger();

            var completed = false;

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to call BeginPatch");
                await BeforeApply();


                // *****************************************************************
                logger.Debug("Attempting to process patch set");
                foreach( var patch in patchSet.GetPatches() )
                {

                    logger.Inspect(nameof(patch.Model), patch.Model);
                    logger.Inspect(nameof(patch.Uid), patch.Uid);

                    var meta = Meta.GetMetaFromAlias(patch.Model);
                    if( meta == null )
                        throw new NotFoundException( $"Could not find Model using Alias ({patch.Model??""})");


                    if (patch.Verb == PatchVerb.Update)
                    {
                        logger.Debug("Attempting to handle entity modification");
                        var obj = await Repository.HandleUpdate( meta.Target, patch.Uid, patch.Properties );
                        if (obj == null)
                            throw new NotFoundException($"Could not find {meta.Target.Name} using Uid {patch.Uid}");

                    }
                    else if (patch.Verb == PatchVerb.Create && !patch.IsMember)
                    {

                        logger.Debug("Attempting to handle model add");
                        await Repository.HandleCreate( meta.Target, patch.Uid, patch.Properties );

                    }
                    else if( patch.Verb == PatchVerb.Create && patch.IsMember && patch.Membership != null )
                    {

                        logger.Debug("Attempting to handle aggregate add");
                        var agg = await Repository.HandleCreate( meta.Target, patch.Uid, patch.Properties );

                        var memmeta = Meta.GetMetaFromAlias(patch.Membership?.Model ?? "");
                        if( memmeta == null )
                            throw new NotFoundException($"Could not find Membership Parent using Alias ({patch.Membership?.Model ?? ""})");


                        var parent = await Repository.HandleRetrieve( memmeta.Target, patch.Membership.Uid );
                        await UpdateAggregate( parent, patch.Membership.Property, patch.Verb, agg );

                    }
                    else if (patch.Verb == PatchVerb.Delete && !patch.IsMember)
                    {

                        logger.Debug("Attempting to handle model remove");
                        await Repository.HandleDelete( meta.Target, patch.Uid );

                    }
                    else if (patch.Verb == PatchVerb.Delete && patch.IsMember && patch.Membership != null )
                    {

                        logger.Debug("Attempting to handle aggregate remove");

                        var memmeta = Meta.GetMetaFromAlias(patch.Membership?.Model ?? "");
                        if( memmeta == null)
                            throw new NotFoundException($"Could not find Membership Parent using Alias ({patch.Membership?.Model ?? ""})");

                        var parent = await Repository.HandleRetrieve( memmeta.Target, patch.Membership.Uid );
                        var agg    = await Repository.HandleRetrieve( meta.Target, patch.Uid );

                        await UpdateAggregate( parent, patch.Membership.Property, patch.Verb, agg );

                        await Repository.HandleDelete(meta.Target, patch.Uid);

                    }


                }


                if( save )
                    await Repository.Save();

                completed = true;

            }
            catch( Exception cause )
            {
                await Repository.Abort();
                logger.Error( cause, "Resolve failed");
                throw;
            }
            finally
            {
                await AfterApply(completed);
                logger.LeaveMethod();
            }


        }


        protected virtual Task AfterApply( bool completed )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.CompletedTask;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }



    }


}

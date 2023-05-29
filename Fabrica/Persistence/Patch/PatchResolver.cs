using System;
using System.Collections.Generic;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;

namespace Fabrica.Persistence.Patch;

public class PatchResolver : CorrelatedObject, IPatchResolver
{


    public PatchResolver(ICorrelation correlation, IModelMetaService meta, IMessageMediator mediator, IMediatorRequestFactory factory) : base(correlation)
    {

        Meta        = meta;
        TheMediator = mediator;
        Factory     = factory;

    }


    private IModelMetaService Meta { get; }
    private IMessageMediator TheMediator { get; }
    private IMediatorRequestFactory Factory { get; }


    public IEnumerable<PatchRequest> Resolve( PatchSet patchSet )
    {

        using var logger = EnterMethod();

        var requests = new List<PatchRequest>();

        // *****************************************************************
        logger.Debug("Attempting to process patch set");
        foreach (var patch in patchSet.GetPatches())
        {

            logger.Inspect(nameof(patch.Model), patch.Model);
            logger.Inspect(nameof(patch.Uid), patch.Uid);

            var meta = Meta.GetMetaFromAlias(patch.Model);
            if (meta == null)
                throw new NotFoundException($"Could not find Model using Alias ({patch.Model ?? ""})");

            logger.Inspect(nameof(meta.Target.Name), meta.Target.Name);

            if( patch.Verb == PatchVerb.Update )
            {
                var request = Factory.GetUpdateRequest(meta.Target, patch.Uid, patch.Properties);
                requests.Add( new PatchRequest(patch,request.Sender) );

            }
            else if( patch.Verb == PatchVerb.Create && !patch.IsMember)
            {

                var request = Factory.GetCreateRequest(meta.Target, patch.Uid, patch.Properties);
                requests.Add(new PatchRequest(patch, request.Sender));

            }
            else if( patch.Verb == PatchVerb.Create && patch.IsMember )
            {

                var parentMeta = Meta.GetMetaFromAlias(patch.Membership.Model);
                if (parentMeta == null)
                    throw new NotFoundException($"Could not find Parent using Alias ({patch.Membership.Model ?? ""})");

                var request = Factory.GetCreateMemberRequest(parentMeta.Target, patch.Membership.Uid, meta.Target, patch.Uid, patch.Properties);
                requests.Add(new PatchRequest(patch, request.Sender));

            }
            else if( patch.Verb == PatchVerb.Delete )
            {

                var request = Factory.GetDeleteRequest( meta.Target, patch.Uid );
                requests.Add(new PatchRequest(patch, request.Sender));

            }
            else
            {
                throw new InvalidOperationException( $"Encountered in valid Patch scenario for Model ({patch.Model}) Uid ({patch.Uid}) Verb ({patch.Verb})");
            }


        }


        logger.Inspect(nameof(requests.Count), requests.Count);

        return requests;


    }




}
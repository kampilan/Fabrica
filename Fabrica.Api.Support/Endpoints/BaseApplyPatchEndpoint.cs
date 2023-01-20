using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseApplyPatchEndpoint<TEntity>: BaseEndpoint where TEntity: class, IModel
{

    protected BaseApplyPatchEndpoint(IEndpointComponent component) : base(component)
    {
    }

    [SwaggerOperation(Summary = "Persist changes", Description = "Persist changes by patch")]
    [HttpPatch("{uid}")]
    public async Task<IActionResult> Handle( [FromRoute] string uid, [FromBody] List<ModelPatch> source )
    {

        if (source == null) throw new ArgumentNullException(nameof(source));

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build patch set");
        var set = new PatchSet();
        set.Add(source);

        logger.Inspect(nameof(set.Count), set.Count);



        // *****************************************************************
        logger.Debug("Attempting to resolve patch set");
        var requests = Resolver.Resolve(set);



        // *****************************************************************
        logger.Debug("Attempting to send request via Mediator");
        var patchResponse = await Mediator.Send(requests);

        logger.Inspect(nameof(patchResponse.HasErrors), patchResponse.HasErrors);



        // *****************************************************************
        logger.Debug("Attempting to BatchResponse for success");
        patchResponse.EnsureSuccess();




        // *****************************************************************
        logger.Debug("Attempting to retrieve entity using Uid");
        var request = new RetrieveEntityRequest<TEntity>
        {
            Uid = uid
        };



        // *****************************************************************
        logger.Debug("Attempting to send request via Mediator");

        var retrieveResponse = await Mediator.Send(request);



        // *****************************************************************
        logger.Debug("Attempting to build action result");

        var result = BuildResult(retrieveResponse);



        // *****************************************************************
        return result;

    }



}


public abstract class BaseApplyPatchEndpoint : BaseEndpoint
{

    protected BaseApplyPatchEndpoint(IEndpointComponent component) : base(component)
    {
    }

    [SwaggerOperation(Summary = "Persist changes", Description = "Persist changes by patch")]
    [HttpPatch]
    public async Task<IActionResult> Handle( [FromBody] List<ModelPatch> source )
    {

        if (source == null) throw new ArgumentNullException(nameof(source));

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build patch set");
        var set = new PatchSet();
        set.Add(source);

        logger.Inspect(nameof(set.Count), set.Count);



        // *****************************************************************
        logger.Debug("Attempting to resolve patch set");
        var requests = Resolver.Resolve(set);



        // *****************************************************************
        logger.Debug("Attempting to send request via Mediator");
        var patchResponse = await Mediator.Send(requests);

        logger.Inspect(nameof(patchResponse.HasErrors), patchResponse.HasErrors);



        // *****************************************************************
        logger.Debug("Attempting to BatchResponse for success");
        patchResponse.EnsureSuccess();



        // *****************************************************************
        logger.Debug("Attempting to build action result");
        var result = Ok();


        // *****************************************************************
        return result;

    }



}
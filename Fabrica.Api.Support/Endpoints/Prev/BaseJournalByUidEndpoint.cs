using Fabrica.Api.Support.ActionResult;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints.Prev;

public abstract class BaseJournalByUidEndpoint<TTarget> : BaseEndpoint where TTarget : class, IModel
{


    protected BaseJournalByUidEndpoint(IEndpointComponent component) : base(component)
    {
    }

    [SwaggerOperation(Summary = "Journal", Description = "Audit Journal for Uid")]

    [HttpGet("{uid}/journal")]
    public async Task<IActionResult> Handle([FromRoute] string uid)
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to dispatch request");
        var request = new AuditJournalStreamRequest
        {
            Entity = typeof(TTarget).FullName ?? "",
            EntityUid = uid
        };



        // *****************************************************************
        logger.Debug("Attempting to send request via Mediator");
        var response = await Mediator.Send(request);

        logger.Inspect(nameof(response.Ok), response.Ok);



        // *****************************************************************
        logger.Debug("Attempting to build result");
        var result = response.Ok ? new JsonStreamResult(response.Value) : BuildErrorResult(response);



        // *****************************************************************
        return result;


    }



}
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseRetrieveByUidEndpoint<TEntity>: BaseEndpoint where TEntity: class, IModel
{

    protected BaseRetrieveByUidEndpoint( IEndpointComponent component) : base( component )
    {
    }


    [HttpGet("{uid}")]
    [SwaggerOperation(Summary = "By UID", Description = "Retrieve by Uid")]
    public virtual async Task<IActionResult> Handle([FromRoute, SwaggerParameter(Description = "Uid", Required = true)] string uid )
    {

        using var logger = EnterMethod();


        var request = new RetrieveEntityRequest<TEntity>
        {
            Uid = uid
        };



        // *****************************************************************
        logger.Debug("Attempting to send request via Mediator");
        var response = await Mediator.Send(request);

        logger.Inspect(nameof(response.Ok), response.Ok);



        // *****************************************************************
        logger.Debug("Attempting to build result");
        var result = BuildResult(response);



        // *****************************************************************
        return result;


    }


}
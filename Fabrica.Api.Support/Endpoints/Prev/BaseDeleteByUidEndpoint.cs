using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints.Prev;

public abstract class BaseDeleteByUidEndpoint<TEntity> : BaseEndpoint where TEntity : class, IModel
{


    protected BaseDeleteByUidEndpoint(IEndpointComponent component) : base(component)
    {
    }


    [SwaggerOperation(Summary = "By UID", Description = "Delete by Uid")]
    [HttpDelete("{uid}")]
    public async Task<IActionResult> Handle([FromRoute, SwaggerParameter(Description = "Uid", Required = true)] string uid)
    {

        using var logger = EnterMethod();

        var request = new DeleteEntityRequest<TEntity>
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
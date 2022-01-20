using System.Threading.Tasks;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseDeleteByUidEndpoint<TEntity>: BaseEndpoint where TEntity: class, IModel
{


    protected BaseDeleteByUidEndpoint( IEndpointComponent component ) : base( component )
    {
    }


    [HttpDelete("{uid}")]
    public async Task<IActionResult> Handle( [FromRoute] string uid )
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
using Fabrica.One;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints;

public class BaseMissionEndpoint: BaseEndpoint
{

    public BaseMissionEndpoint(IEndpointComponent component, IMissionContext mission) : base(component)
    {
        Mission = mission;
    }

    private IMissionContext Mission { get; }


    [HttpGet("mission")]
    [SwaggerOperation(Summary = "Mission", Description = "Get the Current Mission Context")]
    public virtual Task<IActionResult> Handle()
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build result");
        IActionResult result = Ok(Mission);



        // *****************************************************************
        return Task.FromResult(result);


    }


}
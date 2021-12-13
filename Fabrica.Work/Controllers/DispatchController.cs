using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Api.Support.ActionResult;
using Fabrica.Api.Support.Controllers;
using Fabrica.One.Persistence.Work;
using Fabrica.Utilities.Container;
using Fabrica.Work.Processor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Work.Controllers;

[Authorize]
[Route("/work")]
public class DispatchController: BaseController
{


    public DispatchController( ICorrelation correlation, WorkRepository repository, IWorkDispatcher dipatcher ) : base(correlation)
    {
        Repository = repository;
        Dispatcher = dipatcher;

    }

    private WorkRepository Repository { get; }
    private IWorkDispatcher Dispatcher { get; }


    protected virtual async Task<JObject> FromBody()
    {

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to parse request body");
            var jo = await JObject.LoadAsync(new JsonTextReader(new StreamReader(Request.Body)));


            // *****************************************************************
            return jo;

        }
        finally
        {
            logger.LeaveMethod();
        }

    }




    [HttpPost("{topic}")]
    public async Task<IActionResult> Post( [FromRoute] string topic, [FromQuery] int delaySecs=0 )
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(topic), topic);
        logger.Inspect(nameof(delaySecs), delaySecs);


        // *****************************************************************
        logger.Debug("Attempting to verify a Topic exists");
        var exists = await Repository.HasTopic( topic );
        if( !exists )
        {
            var error = new ExceptionResult();
            error.ForNotFound( $"Could not find Topic ({topic})" );
            return error;
        }                



        // *****************************************************************
        logger.Debug("Attempting to parse request body");
        var payload = await FromBody();



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new WorkRequest
        {
            Topic   = topic,
            Payload = payload
        };



        // *****************************************************************
        logger.Debug("Attempting to dispatch request");
        var ts = TimeSpan.FromSeconds(delaySecs);
        await Dispatcher.Dispatch( request, ts );



        // *****************************************************************
        return Ok();


    }


}

public class DispatchOptions
{

    public string Topic { get; set; } = "";

    public int DelaySecs { get; set; } = 0;

}
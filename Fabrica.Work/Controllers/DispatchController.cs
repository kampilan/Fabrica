using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Api.Support.Models;
using Fabrica.Mediator;
using Fabrica.Utilities.Container;
using Fabrica.Work.Mediator.Requests;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Work.Controllers;


[ApiExplorerSettings(GroupName = "Dispatch")]
[SwaggerResponse(200, "Success")]
[SwaggerResponse(400, "Bad Request", typeof(ErrorResponseModel))]
[Route("/dispatch")]
public class DispatchController : BaseController
{


    public DispatchController(ICorrelation correlation, IMessageMediator mediator ) : base(correlation)
    {

        Mediator   = mediator;

    }

    private IMessageMediator Mediator { get; }


    [SwaggerOperation(Summary = "Dispatch", Description = "Send a message to be queued for asynchronous processing or make synchronous call to a webhook")]
    [HttpPost("{**catch-all}")]
    public async Task<IActionResult> Process( [FromQuery] int delaySecs = 0, [FromQuery] int timeToLiveSecs = 0 )
    {

        using var logger = EnterMethod();

        var args = new {  delaySecs, timeToLiveSecs };
        logger.LogObject(nameof(args), args);

        logger.Inspect(nameof(Request.Path), Request.Path);
        logger.Inspect(nameof(Request.ContentType), Request.ContentType);
        logger.Inspect(nameof(Request.ContentLength), Request.ContentLength);



        // *****************************************************************
        logger.Debug("Attempting to ensure ContentLength does not exceed maximum length (250KB)");
        if( Request.ContentLength > (250 * 1024) )
        {
            logger.Debug("BadRequest: Payload too Large");
            return new StatusCodeResult(413);
        }



        // *****************************************************************
        logger.Debug("Attempting to check content type if there is any content");
        if( Request.ContentLength > 0 && Request.ContentType != "application/json" )
        {
            logger.Debug("UnsupportedMediaType: Not JSON");
            return new UnsupportedMediaTypeResult();
        }



        // *****************************************************************
        logger.Debug("Attempting to parse request body");
        var jo = new JObject();
        if( Request.ContentLength > 0 )
        {

            using var sr = new StreamReader(Request.Body);
            using var jr = new JsonTextReader(sr);

            jo = await JObject.LoadAsync(new JsonTextReader(new StreamReader(Request.Body)));

        }



        // *****************************************************************
        logger.Debug("Attempting to dig out topic from request path");
        var topic = "";
        if( Request.Path.HasValue )
        {
            var segs = Request.Path.Value.Split("/");
            logger.Inspect(nameof(segs.Length), segs.Length);
            if( segs.Length > 2 )
                topic = string.Join( "-", segs[2..] );
        }

        logger.Inspect(nameof(topic), topic);




        // *****************************************************************
        logger.Debug("Attempting to check for a non-whitespace topic ");
        if (string.IsNullOrWhiteSpace(topic))
        {
            logger.Debug("BadRequest: Topic not valid");
            return new BadRequestResult();
        }



        // *****************************************************************
        logger.Debug("Attempting to build DispatchWorkRequest");
        var request = new DispatchWorkRequest
        {
            TopicName     = topic,
            Payload       = jo,
            DeliveryDelay = TimeSpan.FromSeconds(delaySecs),
            TimeToLive    = TimeSpan.FromSeconds(timeToLiveSecs)
        };



        // *****************************************************************
        logger.Debug("Attempting to send request to Mediator");
        var response = await Mediator.Send(request);

        if (!response.Ok)
            return BuildErrorResult(response);



        // *****************************************************************
        return BuildResult(response);

    }



}


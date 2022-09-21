using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fabrica.Api.Support.ActionResult;
using Fabrica.Api.Support.Controllers;
using Fabrica.Exceptions;
using Fabrica.One.Persistence.Contexts;
using Fabrica.One.Work.Processor;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.One.Controllers;

[Authorize]
[Route("/work")]
public class DispatchController: BaseController
{


    public DispatchController( ICorrelation correlation, WorkDbContext context, IWorkDispatcher dispatcher, IHttpClientFactory factory ) : base(correlation)
    {

        Context    = context;
        Dispatcher = dispatcher;
        Factory    = factory;

    }

    private WorkDbContext Context { get; }
    private IWorkDispatcher Dispatcher { get; }
    private IHttpClientFactory Factory { get; }


    [HttpPost("{topic}")]
    public async Task<IActionResult> Post( [FromRoute] string topic, [FromQuery] int delaySecs=0 )
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(topic), topic);
        logger.Inspect(nameof(delaySecs), delaySecs);



        // *****************************************************************
        logger.Debug("Attempting to verify a Topic exists");
        var model = await Context.WorkTopics.SingleOrDefaultAsync(e => e.Topic == topic);
        if( model is null )
        {
            var error = new ExceptionResult();
            error.ForNotFound( $"Could not find Topic ({topic})" );
            return error;
        }                



        // *****************************************************************
        logger.Debug("Attempting to parse request body");
        var jo = await JObject.LoadAsync(new JsonTextReader(new StreamReader(Request.Body)));

        IActionResult result;
        if (model.Synchronous)
            result= await Process( model.FullUrl, model.Path, jo );
        else
            result = await Dispatch( topic, jo, TimeSpan.FromSeconds(delaySecs) );


        // *****************************************************************
        return result;

    }


    private async Task<IActionResult> Dispatch( string topic, JObject payload, TimeSpan delay )
    {

        using var logger = EnterMethod();

        try
        {


            // *****************************************************************
            logger.Debug("Attempting to build request");
            var request = new WorkRequest
            {
                Topic = topic,
                Payload = payload
            };



            // *****************************************************************
            logger.Debug("Attempting to dispatch request");
            await Dispatcher.Dispatch( request, delay );



            // *****************************************************************
            return new OkResult();

        }
        catch (Exception cause)
        {

            logger.Error( cause, "Dispatch failed." );
            var result = new ExceptionResult
            {
                Kind = ErrorKind.Functional,
                Explanation = "Failed to Queue Asynchronous Work Request"
            };

            return result;
        }

    }


    private async Task<IActionResult> Process( string endpoint, string path, JObject payload )
    {

        using var logger = EnterMethod();

        try
        {


            // *****************************************************************
            logger.Debug("Attempting to serialize payload");
            var body = payload.ToString(Formatting.None);
            var content = new StringContent(body, Encoding.UTF8, "application/json");



            // *****************************************************************
            logger.Debug("Attempting to build HTTP request");
            var httpReq = new HttpRequestMessage( HttpMethod.Post, path );
            httpReq.Content = content;



            // *****************************************************************
            logger.Debug("Attempting to setup HTTP client");
            using var client = Factory.CreateClient();
            client.BaseAddress = new Uri(endpoint);



            // *****************************************************************
            logger.Debug("Attempting to send HTTP request");
            var httpRes = await client.SendAsync(httpReq);

            logger.Inspect(nameof(httpRes.StatusCode), httpRes.StatusCode);

            httpRes.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to read JSON output");
            var output = await httpRes.Content.ReadAsStringAsync();



            // *****************************************************************
            logger.Debug("Attempting to result");
            var result = new ContentResult
            {
                StatusCode  = 200,
                ContentType = "application/json",
                Content     = output
            };



            // *****************************************************************
            return result;


        }
        catch (Exception cause)
        {

            logger.Error( cause, "Process failed");

            var result = new ExceptionResult
            {
                Kind = ErrorKind.Functional,
                Explanation = "Failed to Process Synchronous Work Request"
            };

            return result;

        }


    }



}

public class DispatchOptions
{

    public string Topic { get; set; } = "";

    public int DelaySecs { get; set; } = 0;

}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Exceptions;
using Fabrica.Fake.Persistence;
using Fabrica.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Utilities.Container;
using Fabrica.Work.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Fake.Controllers;


[Route("/webhooks")]
public class WebhookController: BaseController
{


    public WebhookController(ICorrelation correlation, IPatchResolver resolver, FakeReplicaDbContext context, IHttpClientFactory factory ) : base(correlation)
    {

        Resolver = resolver;
        Context  = context;
        Factory = factory;

        Context.Database.EnsureCreated();

    }

    private IPatchResolver Resolver { get; }
    private FakeReplicaDbContext Context { get; }
    private IHttpClientFactory Factory { get; }


    [HttpPost("test-topic")]
    public IActionResult HandleTest( [FromBody] TestPayload payload )
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(payload), payload);

        var response = new Response();
        response.WithDetail(new EventDetail {Category = EventDetail.EventCategory.Info, Explanation = "It seems to have worked"});
        response.IsOk();

        return Ok(response);

    }


    [HttpPost("Rouchefoucauld")]
    public IActionResult HandlePing()
    {

        using var logger = EnterMethod();


        var utc = DateTime.UtcNow;

        DateTime Convert(int diff)
        {

            var dt = utc.AddHours(diff);

            return dt;

        }

        var dict = new Dictionary<string, DateTime>
        {
            ["Beverly Hills"] = Convert(-7),
            ["Monte Carlo"]   = Convert(2),
            ["London"]        = Convert(1),
            ["Paris"]         = Convert(2),
            ["Rome"]          = Convert(2),
            ["Gstaad"]        = Convert(2)
        };


        var response = new Response<Dictionary<string,DateTime>>(dict);
        response.WithDetail(new EventDetail
        {
            Category = EventDetail.EventCategory.Info, Explanation = "Time according to the Rouchefoucauld", Group = "Tests",
            Source = "Louis Winthorpe III"
        });
        response.IsOk();

        return Ok(response);

    }





    [HttpPost("ingest/test")]
    public async Task<IActionResult> AcceptWorkRequest([FromRoute] string topic, [FromBody] IngestionEvent s3)
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(topic), topic);
        logger.LogObject(nameof(s3), s3);


        using var client = Factory.CreateClient();

        var contents = await client.GetStringAsync(s3.Endpoint);

        logger.LogJson("contents", contents );


        // *****************************************************************
        return Ok();


    }

    [HttpPost("ingest/error")]
    public async Task<IActionResult> AcceptBadWorkRequest([FromRoute] string topic, [FromBody] IngestionEvent s3)
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(topic), topic);
        logger.LogObject(nameof(s3), s3);

        using var client = Factory.CreateClient();

        var contents = await client.GetStringAsync(s3.Endpoint);

        logger.LogJson("contents", contents);

        return BadRequest();

    }


}


public class TestPayload
{

    [DefaultValue(0)]
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; } = DateTime.MinValue;

}


using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Api.Support.Controllers;
using Fabrica.Exceptions;
using Fabrica.Fake.Persistence;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Patch;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Fake.Controllers;


[Route("/webhooks")]
public class WebhookController: BaseController
{


    public WebhookController(ICorrelation correlation, IPatchResolver resolver, FakeReplicaDbContext context, IAmazonS3 client ) : base(correlation)
    {

        Resolver = resolver;
        Context = context;
        Client = client;

        Context.Database.EnsureCreated();

    }

    private IPatchResolver Resolver { get; }
    private FakeReplicaDbContext Context { get; }
    private IAmazonS3 Client { get; }


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



    [HttpPost("s3-events/{topic}")]
    public async Task<StatusCodeResult> AcceptWorkRequest([FromRoute] string topic, [FromBody] S3Event s3)
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(topic), topic);
        logger.LogObject(nameof(s3), s3);



        // *****************************************************************
        logger.Debug("Attempting to build delete object request");
        var request = new DeleteObjectRequest
        {
            BucketName = s3.Bucket,
            Key = s3.Key
        };



        // *****************************************************************
        logger.Debug("Attempting to send request to S3");
        var response = await Client.DeleteObjectAsync(request);

        logger.LogObject(nameof(response), response);



        // *****************************************************************
        return Ok();

    }

    [HttpPost("s3-events-bad/{topic}")]
    public Task<StatusCodeResult> AcceptBadWorkRequest([FromRoute] string topic, [FromBody] S3Event s3)
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(topic), topic);
        logger.LogObject(nameof(s3), s3);

        return Task.FromResult(new StatusCodeResult(422));

    }

    [HttpPost("s3-events-transient/{topic}")]
    public Task<StatusCodeResult> AcceptTransientWorkRequest([FromRoute] string topic, [FromBody] S3Event s3)
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(topic), topic);
        logger.LogObject(nameof(s3), s3);

        return Task.FromResult(new StatusCodeResult(500));

    }


}


public class TestPayload
{

    [DefaultValue(0)]
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; } = DateTime.MinValue;

}

public class S3Event
{

    public string Region { get; set; }
    public string Bucket { get; set; }
    public string Key { get; set; }

    public long Size { get; set; }

}

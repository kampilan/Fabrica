using System.Net;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Utilities.Container;
using Fabrica.Work.Mediator.Requests;
using Fabrica.Work.Persistence.Contexts;
using Fabrica.Work.Persistence.Entities;
using Fabrica.Work.Processor;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Work.Mediator.Handlers;


public class DispatchWorkHandler : AbstractRequestHandler<DispatchWorkRequest, JToken>
{

    public DispatchWorkHandler(ICorrelation correlation, IWorkDispatcher dispatcher, IHttpClientFactory factory, WorkDbContext context) : base(correlation)
    {

        Dispatcher = dispatcher;
        Factory = factory;
        Context = context;

    }

    private IWorkDispatcher Dispatcher { get; }
    private IHttpClientFactory Factory { get; }
    private WorkDbContext Context { get; }


    protected override async Task<JToken> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to fetch WorkTopic by Name");
        var model = await Context.WorkTopics.SingleOrDefaultAsync(e => e.Topic == Request.TopicName, cancellationToken);
        if (model is null)
            throw new NotFoundException($"Could not find WorkTopic by Name= ({Request.TopicName})");



        // *****************************************************************
        logger.Debug("Attempting to dispatch request");
        string json;
        if (model.Synchronous)
            json =  await Resolve(model, Request.Payload);
        else
            json = await Enqueue(model, Request.Payload, Request.DeliveryDelay, Request.TimeToLive );

        logger.LogJson("Output", json);



        // *****************************************************************
        var token = JToken.Parse( json );

        return token;

    }



    private async Task<string> Resolve(WorkTopic topic, JObject payload)
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to serialize payload");
        var body = payload.ToString(Formatting.None);
        var content = new StringContent(body, Encoding.UTF8, "application/json");



        // *****************************************************************
        logger.Debug("Attempting to build HTTP request");
        var httpReq = new HttpRequestMessage(HttpMethod.Post, topic.Path);
        httpReq.Content = content;



        HttpClient client;
        if (string.IsNullOrWhiteSpace(topic.ClientName) && !string.IsNullOrWhiteSpace(topic.FullUrl))
        {
            logger.Debug("Attempting to setup HTTP client using FullUrl");
            client = Factory.CreateClient();
            client.BaseAddress = new Uri(topic.FullUrl);
        }
        else if (!string.IsNullOrWhiteSpace(topic.ClientName))
        {
            logger.Debug("Attempting to setup HTTP client using ClientName");
            client = Factory.CreateClient(topic.ClientName);
        }
        else
        {
            throw new BadRequestException($"WorkTopic: {topic.Topic} ({topic.Uid}) is not configured correctly. It must have a {nameof(topic.ClientName)}/{nameof(topic.Path)} pair or a valid {nameof(topic.FullUrl)}");
        }



        string output;
        var status = HttpStatusCode.BadRequest;
        try
        {


            // *****************************************************************
            logger.Debug("Attempting to send HTTP request");

            var httpRes = await client.SendAsync(httpReq);

            status = httpRes.StatusCode;

            logger.Inspect(nameof(status), status);



            // *****************************************************************
            logger.Debug("Attempting to read JSON output");
            output = await httpRes.Content.ReadAsStringAsync();

            // *****************************************************************
            logger.Debug("Attempting to dig out content type");
            var contentType = httpRes.Content.Headers.ContentType?.MediaType ?? "";

            logger.Inspect(nameof(contentType), contentType);



            // *****************************************************************
            logger.Debug("Attempting to check is call was successful");
            httpRes.EnsureSuccessStatusCode();



            // *****************************************************************
            if( contentType != "application/json" )
            {
                logger.Debug("Content-Type is not JSON");
                var obj = new { Output = output };
                output = JsonConvert.SerializeObject(obj);
            }



        }
        catch ( Exception cause )
        {
            throw new FunctionalException($"Call to Webhook produced a response not consistent with success ({status})", cause);
        }
        finally
        {
            client.Dispose();
        }


        // *****************************************************************
        return output;


    }


    private async Task<string> Enqueue(WorkTopic topic, JObject payload, TimeSpan delay, TimeSpan timeToLive)
    {

        using var logger = EnterMethod();

        try
        {


            // *****************************************************************
            logger.Debug("Attempting to build request");
            var request = new WorkRequest
            {
                Topic = topic.Topic,
                Payload = payload
            };



            // *****************************************************************
            logger.Debug("Attempting to dispatch request");
            await Dispatcher.Dispatch(request, delay, timeToLive);



            // *****************************************************************
            var body = new { Accepted = true };
            var json = JsonConvert.SerializeObject(body);

            return json;

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Dispatch failed.");
            throw new FunctionalException($"Failed to Enqueue Asynchronous Work Request for Topic {topic.Topic} ({topic.Uid})", cause);
        }


    }


}
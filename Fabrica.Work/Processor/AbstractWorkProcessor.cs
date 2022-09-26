using Fabrica.Identity;
using Fabrica.Work.Persistence.Contexts;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fabrica.Work.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Fabrica.Watch;
using Fabrica.Api.Support.Identity.Proxy;
using System.Net;
using System.Text;

namespace Fabrica.Work.Processor;

public abstract class AbstractWorkProcessor: IWorkProcessor
{


    private class ProcessorArgs
    {

        public WorkRequest Request { get; init; } = null!;

        public string Payload { get; set; } = "";

        public WorkTopic Topic { get; init; } = null!;

        public Func<bool, Task> CompletionHandler { get; init; } = null!;

    }

    protected AbstractWorkProcessor(IHttpClientFactory factory, WorkDbContext context, IAccessTokenSource tokenSource)
    {

        Factory = factory;
        Context = context;
        TokenSource = tokenSource;

    }


    private IHttpClientFactory Factory { get; }
    private WorkDbContext Context { get; }

    private IAccessTokenSource TokenSource { get; }

    public TimeSpan StopTimeout { get; set; } = TimeSpan.FromSeconds(60);


    public int MaximumWorkers { get; set; } = Environment.ProcessorCount;
    private int _workerCounter;



    protected abstract Task<(bool proceed, string payload)> SerializePayload(WorkRequest request);

    protected abstract Task Accepted( WorkRequest request );
    protected abstract Task Rejected( WorkRequest request );

    public async Task Process(WorkRequest request, Func<bool, Task> completionHandler, bool onCallerThread = false)
    {


        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();


            logger.LogObject(nameof(request), request);

            logger.Inspect("MaximumWorkers", MaximumWorkers);
            logger.Inspect("ConcurrentJobs", _workerCounter);

            
            var result = await SerializePayload(request);
            if( !result.proceed )
            {
                await completionHandler(true);
                return;
            }


            // *****************************************************************
            logger.Debug("Attempting to get WorkTopic for requested Topic");
            var topic = await Context.WorkTopics.SingleOrDefaultAsync(e => e.Topic == request.Topic);

            if( topic is null )
            {
                logger.ErrorFormat("Topic ({1}) not found. Request will not be processed. RequestUid ({0}) ", request.Uid, request.Topic);
                return;
            }

            logger.LogObject(nameof(topic), topic);



            var args = new ProcessorArgs
            {
                Request           = request,
                Payload           = result.payload,
                Topic             = topic,
                CompletionHandler = completionHandler
            };


            if (!onCallerThread && _workerCounter < MaximumWorkers)
            {
                logger.Debug("Attempting to submit request to thread pool");
                Interlocked.Increment(ref _workerCounter);

                ThreadPool.QueueUserWorkItem(CallBack!);

                async void CallBack(object _) => await _doWork(args);

            }
            else
            {
                logger.Debug("Attempting to process request on caller thread");
                await _doWork(args);
            }


        }
        finally
        {
            logger.LeaveMethod();
        }


    }


    private async Task _doWork(ProcessorArgs args)
    {


        using var logger = this.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build and send request");
        try
        {


            var content = new StringContent(args.Payload, Encoding.UTF8, "application/json");


            // *****************************************************************
            logger.Debug("Attempting to build Request message");
            var message = new HttpRequestMessage(HttpMethod.Post, args.Topic.Path)
            {
                Content = content
            };



            // *****************************************************************
            logger.Debug("Attempting to create HTTP client from factory");
            HttpClient client;
            if (string.IsNullOrWhiteSpace(args.Topic.FullUrl))
            {

                var token = await TokenSource.GetToken();
                message.Headers.Add(TokenConstants.HeaderName, token);

                client = Factory.CreateClient(args.Topic.ClientName);

            }
            else
            {

                client = Factory.CreateClient();
                client.BaseAddress = new Uri(args.Topic.FullUrl);

            }



            // *****************************************************************
            using (client)
            {
                logger.Debug("Attempting to send message ");
                var response = await client.SendAsync(message);

                logger.Inspect(nameof(response.RequestMessage.RequestUri), response.RequestMessage?.RequestUri);
                logger.Inspect(nameof(response.IsSuccessStatusCode), response.IsSuccessStatusCode);
                logger.Inspect(nameof(response.StatusCode), response.StatusCode);

                response.EnsureSuccessStatusCode();

            }


            logger.Debug("Attempting to call the completion handler");
            await args.CompletionHandler(true);


            await Accepted(args.Request);


        }
        catch (HttpRequestException cause) when (cause.StatusCode == HttpStatusCode.BadRequest)
        {

            logger.Debug("Attempting to call the completion handler for Unrecoverable error");
            await args.CompletionHandler(true);

            await Rejected(args.Request);

        }
        catch( HttpRequestException cause) when (cause.StatusCode == HttpStatusCode.InternalServerError )
        {

            logger.Debug("Attempting to call the completion handler for Unrecoverable error");
            await args.CompletionHandler(false);

        }
        catch (Exception cause)
        {
            var ctx = new { args.Request.Topic, args.Request.Uid };
            logger.ErrorWithContext(cause, ctx, "Send failed");

            await args.CompletionHandler(false);

        }
        finally
        {
            Interlocked.Decrement(ref _workerCounter);
        }


    }








}
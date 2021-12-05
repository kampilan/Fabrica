/*
The MIT License (MIT)

Copyright (c) 2021 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Identity;
using Fabrica.One.Persistence.Work;
using Fabrica.Watch;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Work.Processor
{


    public class WorkProcessor : IWorkProcessor
    {

        private class ProcessorArgs
        {
            public WorkRequest Request { get; init; }
            public Action<bool> CompletionHandler { get; init; }
        }


        public WorkProcessor( IHttpClientFactory factory, WorkRepository repository, IAccessTokenSource tokenSource )
        {

            Factory     = factory;
            Repository  = repository;
            TokenSource = tokenSource;

        }


        private IHttpClientFactory Factory { get; }
        private WorkRepository Repository { get; }

        private IAccessTokenSource TokenSource { get; }

        public TimeSpan StopTimeout { get; set; } = TimeSpan.FromSeconds(60);


        public int MaximumWorkers { get; set; } = Environment.ProcessorCount;
        private int _workerCounter;



        public async Task Process( WorkRequest request, Action<bool> completionHandler, bool onCallerThread = false )
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.LogObject(nameof(request), request);

                logger.Inspect("MaximumWorkers", MaximumWorkers);
                logger.Inspect("ConcurrentJobs", _workerCounter);


                var args = new ProcessorArgs
                {
                    Request = request,
                    CompletionHandler = completionHandler
                };


                if (!onCallerThread && (_workerCounter < MaximumWorkers))
                {
                    logger.Debug("Attempting to submit request to thread pool");
                    Interlocked.Increment(ref _workerCounter);

                    ThreadPool.QueueUserWorkItem(CallBack);

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


        private async Task _doWork([NotNull] ProcessorArgs args)
        {


            using var logger = this.EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to make sure request is valid and complete");
            args.Request.Payload ??= new JObject();



            // *****************************************************************
            logger.Debug("Attempting to build and send request");
            try
            {


                // *****************************************************************
                logger.Debug("Attempting to get TopicEndpoint for requested Topic");
                var topic = await Repository.GetTopic( args.Request.Topic );

                logger.LogObject(nameof(topic), topic);



                // *****************************************************************
                logger.Debug("Attempting to build Content from Json Payload");
                var payload = args.Request.Payload.ToString(Formatting.None);
                var content = new StringContent(payload);

                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");



                // *****************************************************************
                logger.Debug("Attempting to build Request message");
                var message = new HttpRequestMessage(HttpMethod.Post, topic.Path)
                {
                    Content = content
                };



                // *****************************************************************
                logger.Debug("Attempting to create HTTP client from factory");
                HttpClient client;
                if( string.IsNullOrWhiteSpace(topic.Endpoint) )
                {

                    var token = await TokenSource.GetToken();
                    message.Headers.Add(TokenConstants.HeaderName, token);

                    client = Factory.CreateClient("WebhookEndpoint");

                }
                else
                {

                    client = Factory.CreateClient();
                    client.BaseAddress = new Uri(topic.Endpoint);

                }


                // *****************************************************************
                using ( client )
                {

                    logger.Debug("Attempting to send message ");
                    var response = await client.SendAsync(message);

                    logger.Inspect(nameof(response.IsSuccessStatusCode), response.IsSuccessStatusCode);
                    logger.Inspect(nameof(response.StatusCode), response.StatusCode);

                    response.EnsureSuccessStatusCode();

                }


                logger.Debug("Attempting to call the completion handler");
                args.CompletionHandler(true);


            }
            catch (HttpRequestException cause)
            {

                if( cause.StatusCode == HttpStatusCode.UnprocessableEntity )
                {
                    logger.Debug("Attempting to call the completion handler for Unrecoverable error");
                    args.CompletionHandler(true);
                }

            }
            catch( Exception cause )
            {
                var ctx = new {args.Request.Topic, args.Request.Uid};
                logger.ErrorWithContext( cause, ctx, "Send failed" );
            }
            finally
            {
                Interlocked.Decrement(ref _workerCounter);
            }


        }


    }


}

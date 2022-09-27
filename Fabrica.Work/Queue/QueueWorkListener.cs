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
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Work.Processor;
using Fabrica.Work.Processor.Parsers;

namespace Fabrica.Work.Queue;

public class QueueWorkListener<TParser,TProcessor> : IRequiresStart, IDisposable where TParser: IMessageBodyParser where TProcessor: IWorkProcessor
{


    public QueueWorkListener( ILifetimeScope rootScope, IQueueComponent queue )
    {

        RootScope = rootScope;
        Queue     = queue;

    }


    private ILifetimeScope RootScope { get; }
    private IQueueComponent Queue { get; }

    private Task Listener { get; set; } = null!;
    private CancellationTokenSource MustStop { get; set; } = null!;



    public string QueueName { get; set; } = "";

    public TimeSpan PollingDuration { get; set; } = TimeSpan.FromSeconds(20);
    public TimeSpan AcknowledgementTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan StopTimeout { get; set; } = TimeSpan.FromSeconds(30);



    public Task Start()
    {

        using var logger = this.EnterMethod();


        MustStop = new CancellationTokenSource();


        logger.Debug("Attempting to start listener thread");

        Listener = Task.Run(async () => await _listen());

        logger.Debug("QueueWorkListener start completed");


        return Task.CompletedTask;

    }


    public void Dispose()
    {

        using var logger = this.EnterMethod();



        logger.Debug("Signal thread to stop");
        MustStop.Cancel();

        logger.DebugFormat("Waiting {0} second(s) for thread to stop", StopTimeout);


        if (Listener.Wait(StopTimeout))
            logger.Debug("Thread successfully stopped");
        else
            logger.Warning("Timeout occurred before thread stopped");


    }


    private async Task _listen()
    {

        while (!MustStop.IsCancellationRequested)
        {


            // **********************************************************
            IQueueItem message;
            try
            {

                message = await Queue.DequeueAsync(QueueName, PollingDuration, AcknowledgementTimeout, MustStop.Token);
                if( message == null! )
                    continue;

            }
            catch (TaskCanceledException)
            {
                continue;
            }
            catch (Exception cause)
            {
                this.GetLogger().ErrorFormat(cause, "DequeueAsync failed for Queue: {0}", QueueName);
                Thread.Sleep(1000);
                continue;
            }


            WorkRequest request = null!;

            // **********************************************************
            var logger = this.GetLogger();
            try
            {

                logger.EnterScope($"Queue: {QueueName} Message: {message.Id}");



                // *********************************************************
                var body = message.Payload;
                logger.Inspect("body", body);


                // *********************************************************
                await using (var scope = RootScope.BeginLifetimeScope())
                {

                    var parser    = scope.Resolve<TParser>();
                    var processor = scope.Resolve<TProcessor>();

                    logger.Debug("Attempting to build request from message body");
                    var result = await parser.Parse(body);
                    if (result.ok)
                    {

                        request = result.request!;


                        // *********************************************************
                        logger.Debug("Attempting to submit request to processor");
                        await processor.Process(request, async ok => await _onCompletion(ok, message.ReceiptHandle));

                    }


                }



                // *********************************************************
                logger.Debug("Submission completed");



            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, new { Message = message, Request = request }, $"Failed to process queue item from Queue: {QueueName} Message: {message.Id}");
            }
            finally
            {
                logger.LeaveScope($"Queue: {QueueName} Message: {message.Id}");
            }


        }


    }


    private async Task _onCompletion(bool succeeded, string receiptHandle)
    {


        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();



            // **********************************************************
            if (!succeeded)
                return;



            // **********************************************************
            logger.Debug("Attempting to acknowledge (delete) message");
            await Queue.AcknowledgeAsync(QueueName, receiptHandle);


        }
        catch (Exception cause)
        {
            logger.ErrorFormat(cause, "DeleteMessage failed for Queue: {0} ReceiptHandle: {1}", QueueName, receiptHandle);
        }
        finally
        {
            logger.LeaveMethod();
        }


    }


}
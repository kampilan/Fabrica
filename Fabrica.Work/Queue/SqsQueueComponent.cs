/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

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
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.Work.Queue
{

    public class SqsQueueComponent: IQueueComponent
    {

        public SqsQueueComponent( [NotNull] IAmazonSQS client )
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }


        private IAmazonSQS Client { get; }



        public async Task<string> CheckQueueAsync( [NotNull] string queue )
        {

            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queue));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // **************************************************
                try
                {
                    var responseChk = await Client.GetQueueUrlAsync(queue);
                    logger.LogObject(nameof(responseChk), responseChk);

                    if (responseChk.HttpStatusCode == HttpStatusCode.OK)
                        return responseChk.QueueUrl;

                }
                catch (Exception cause )
                {
                    logger.Debug( cause, "Failed to get Queue url");
                }



                // **************************************************
                var request = new CreateQueueRequest
                {
                    QueueName = queue,
                };

                request.Attributes.Add( "MessageRetentionPeriod", "300" );
                request.Attributes.Add( "VisibilityTimeout", "5" );



                var response = await Client.CreateQueueAsync(request);
                logger.LogObject( nameof(response), response );

                var url = response.QueueUrl;



                // **************************************************
                return url;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        public async Task EnqueueAsync([NotNull] string queue, [NotNull] string payload, TimeSpan deliveryDelay)
        {

            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queue));
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(payload));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                if (string.IsNullOrWhiteSpace(queue))
                    throw new ArgumentNullException(nameof(queue));

                if (string.IsNullOrWhiteSpace(payload))
                    throw new ArgumentNullException(nameof(payload));

                logger.Inspect("queue", queue);
                logger.LogJson("payload", payload);
                logger.Inspect("deliveryDelay (seconds)", deliveryDelay.TotalSeconds);



                // **************************************************************************
                logger.Debug("Attempting to get Queue Url");
                logger.Inspect("queue", queue);

                var url = await CheckQueueAsync(queue);
                logger.Inspect("url", url);



                // **************************************************************************
                logger.Debug("Attempting to build request");

                int delay = 0;
                if (deliveryDelay != TimeSpan.MinValue)
                    delay = Convert.ToInt32(deliveryDelay.TotalSeconds);

                var request = new SendMessageRequest
                {
                    QueueUrl = url,
                    DelaySeconds = delay,
                    MessageBody = payload
                };

                logger.LogObject("request", request);



                // **************************************************************************
                logger.Debug("Attempting to call SendMessage with request");

                var response = await Client.SendMessageAsync(request);

                logger.LogObject("Response", response);



                // **************************************************************************
                logger.Debug("Enqueue completed");


            }
            catch (Exception cause)
            {
                logger.ErrorFormat(cause, "Enqueue failed");
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }


        }




        public async Task EnqueueAsync([NotNull] string queue, [NotNull] string payload)
        {

            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(queue));
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(payload));


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                await EnqueueAsync(queue, payload, TimeSpan.MinValue);

            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        public async Task<IQueueItem> DequeueAsync( string queue, TimeSpan waitTimeout, TimeSpan acknowledgmentTimeout, CancellationToken ct=default(CancellationToken) )
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                if (string.IsNullOrWhiteSpace(queue))
                    throw new ArgumentNullException(nameof(queue));

                logger.Inspect("queue", queue);
                logger.Inspect("waitTimeout (seconds)", waitTimeout.TotalSeconds);
                logger.Inspect("acknowledgementTimeout (seconds)", acknowledgmentTimeout.TotalSeconds);



                // **************************************************************************
                logger.Debug("Attempting to get Queue Url");
                logger.Inspect("queue", queue);

                var url = await CheckQueueAsync(queue);
                logger.Inspect("url", url);



                // **************************************************************************
                logger.Debug("Attempting to build request");

                var wait = Convert.ToInt32(waitTimeout.TotalSeconds);
                var ack  = Convert.ToInt32(acknowledgmentTimeout.TotalSeconds);
                var request = new ReceiveMessageRequest
                {
                    QueueUrl            = url,
                    MaxNumberOfMessages = 1,
                    VisibilityTimeout   = ack,
                    WaitTimeSeconds     = wait
                };

                logger.LogObject("request", request);



                // **************************************************************************
                logger.Debug("Attempting to call ReceiveMessage with request");

                var response = await Client.ReceiveMessageAsync( request, ct );

                logger.LogObject("Response", response);





                // **************************************************************************
                logger.Debug("Attempting to call handle response");

                if( (response.HttpStatusCode == HttpStatusCode.OK) && (response.Messages.Count > 0) )
                {


                    // **************************************************************************
                    logger.Debug("Attempting to populate QueueItem");

                    var msg = response.Messages[0];
                    logger.LogObject( "Received Message", msg );

                    IQueueItem item = new QueueItem
                    {
                        Url           = url,
                        Id            = msg.MessageId,
                        ReceiptHandle = msg.ReceiptHandle,
                        Payload       = msg.Body
                    };



                    // **************************************************************************
                    return item;


                }



                // **************************************************************************
                logger.Debug("No message received");


                return null;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        public async Task AcknowledgeAsync(IQueueItem item)
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // **************************************************************************
                logger.Debug("Attempting to build request");

                var request = new DeleteMessageRequest
                {
                    QueueUrl = item.Url,
                    ReceiptHandle = item.ReceiptHandle
                };

                logger.LogObject("request", request);



                // **************************************************************************
                logger.Debug("Attempting to call DeleteMessage with request");

                var response = await Client.DeleteMessageAsync(request);

                logger.LogObject("response", response);



            }
            finally
            {
                logger.LeaveMethod();
            }


        }





        public async Task AcknowledgeAsync(string queue, string receiptHandle)
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // **************************************************************************
                logger.Debug("Attempting to get Queue Url");
                logger.Inspect("queue", queue);

                var url = await CheckQueueAsync(queue);
                logger.Inspect("url", url);




                // **************************************************************************
                logger.Debug("Attempting to build request");

                var request = new DeleteMessageRequest
                {
                    QueueUrl = url,
                    ReceiptHandle = receiptHandle
                };

                logger.LogObject("request", request);



                // **************************************************************************
                logger.Debug("Attempting to call DeleteMessage with request");

                var response = await Client.DeleteMessageAsync(request);

                logger.LogObject("response", response);



            }
            finally
            {
                logger.LeaveMethod();
            }


        }






    }


}

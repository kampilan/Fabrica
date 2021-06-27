using System;
using System.Net.Http;
using Amazon.SQS;
using Autofac;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Aws;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Work.Processor;
using Fabrica.Work.Queue;
using Fabrica.Work.Topics;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;

namespace Fabrica.Work.Appliance
{

    
    public class TheModule: Module, IAwsCredentialModule, IWorkModule
    {


        public string Profile { get; set; } = "";
        public string RegionName { get; set; } = "";

        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";

        public bool RunningOnEC2 { get; set; } = true;


        public string WorkQueueName { get; set; } = "";

        public int PollingDurationSecs { get; set; } = 20;
        public int AcknowledgementTimeoutSecs { get; set; } = 30;


        public string WebhookEndpoint { get; set; } = "";


        public string TokenSigningKey { get; set; } = "";
        public string IdentitySubject { get; set; } = "";
        public string IdentityName { get; set; } = "";


        protected override void Load(ContainerBuilder builder)
        {

            using var logger = this.EnterMethod();

            logger.LogObject( "TheModule", this );


            builder.AddCorrelation();


            builder.UseAws(this);

            builder.Register(c =>
                {

                    var sqs = c.Resolve<IAmazonSQS>();

                    var comp = new SqsQueueComponent(sqs);
                    
                    return comp;

                })
                .As<IQueueComponent>()
                .SingleInstance();



            builder.AddHttpClient( "WebhookEndpoint", WebhookEndpoint );

            builder.AddProxyTokenEncoder(TokenSigningKey);

            builder.Register(c =>
                {

                    var config  = c.Resolve<IConfiguration>();
                    var section = config.GetSection("Topics");
 
                    var corr = c.Resolve<ICorrelation>();
                    
                    var comp = new TopicMap( corr );
                    comp.Load( "WebhookEndpoint", section );

                    return comp;

                })
                .AsSelf()
                .As<ITopicMap>()
                .SingleInstance()
                .AutoActivate();


            builder.Register( c =>
                {



                    var claims = new ClaimSetModel
                    {
                        Subject = IdentitySubject,
                        Name    = IdentityName
                    };

                    var encoder = c.Resolve<IProxyTokenEncoder>();
                    var token   = encoder.Encode(claims);

                    var comp = new StaticAccessTokenSource(token);

                    return comp;

                })
                .As<IAccessTokenSource>()
                .SingleInstance();



            builder.Register(c =>
                {


                    var factory     = c.Resolve<IHttpClientFactory>();
                    var map         = c.Resolve<TopicMap>();
                    var tokenSource = c.Resolve<IAccessTokenSource>();

                    var comp = new WorkProcessor( factory, map, tokenSource );

                    return comp;

            })
                .As<IWorkProcessor>()
                .SingleInstance()
                .AutoActivate();


            builder.Register(c =>
            {

                var queue     = c.Resolve<IQueueComponent>();
                var processor = c.Resolve<IWorkProcessor>();

                var comp = new QueueWorkListener(queue, processor)
                {
                    QueueName              = WorkQueueName,
                    PollingDuration        = TimeSpan.FromSeconds(PollingDurationSecs),
                    AcknowledgementTimeout = TimeSpan.FromSeconds(AcknowledgementTimeoutSecs)
                };

                return comp;

            })
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();



            // ********************************************************
            builder.Register(c =>
            {

                var queue = c.Resolve<IQueueComponent>();

                var comp = new WorkDispatcher(queue)
                {
                    DefaultQueue = WorkQueueName
                };

                return comp;

            })
                .As<IWorkDispatcher>()
                .SingleInstance()
                .AutoActivate();


        }


    }


}




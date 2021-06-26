using System;
using System.Collections.Generic;
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


            builder.Register(c =>
                {

                    var config = c.Resolve<IConfiguration>();

                    var dict = config.GetSection("Topics").Get<Dictionary<string, string>>();
                    var comp = new TopicMap();
                    comp.Load(dict);

                    return comp;

                })
                .AsSelf()
                .SingleInstance()
                .AutoActivate();


            builder.Register( _ =>
                {

                    byte[] key = null;
                    if (!string.IsNullOrWhiteSpace(TokenSigningKey))
                        key = Convert.FromBase64String(TokenSigningKey);

                    var encoder = new ProxyTokenJwtEncoder
                    {
                        TokenSigningKey = key
                    };

                    var claims = new ClaimSetModel
                    {
                        Subject = IdentitySubject,
                        Name    = IdentityName
                    };


                    var token = encoder.Encode(claims);

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

                    var comp = new WorkProcessor( factory, "WebhookEndpoint", map, tokenSource );

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




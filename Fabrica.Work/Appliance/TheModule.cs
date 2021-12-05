﻿using System;
using System.Net.Http;
using Amazon.SQS;
using Autofac;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Aws;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.One.Persistence;
using Fabrica.One.Persistence.Work;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Work.Processor;
using Fabrica.Work.Processor.Parsers;
using Fabrica.Work.Queue;
using Module = Autofac.Module;

namespace Fabrica.Work.Appliance
{

    
    public class TheModule: Module, IAwsCredentialModule, IWorkModule, IOnePersistenceModule
    {


        public string Profile { get; set; } = "";
        public string RegionName { get; set; } = "";

        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";

        public bool RunningOnEC2 { get; set; } = true;


        public string OneStoreUri { get; set; } = "";
        public string OneDatabase { get; set; } = "fabrica_one";


        public string WorkQueueName { get; set; } = "";
        public string S3EventQueueName { get; set; } = "";

        public int PollingDurationSecs { get; set; } = 20;
        public int AcknowledgementTimeoutSecs { get; set; } = 30;


        public string WebhookEndpoint { get; set; } = "http://localhost:8080";


        public string TokenSigningKey { get; set; } = "";
        public string IdentitySubject { get; set; } = "";
        public string IdentityName { get; set; } = "";


        protected override void Load(ContainerBuilder builder)
        {

            using var logger = this.EnterMethod();

            logger.LogObject( "TheModule", this );


            builder.AddCorrelation();

            builder.UseOnePersitence( OneStoreUri, OneDatabase );

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
                    var repository  = c.Resolve<WorkRepository>();
                    var tokenSource = c.Resolve<IAccessTokenSource>();

                    var comp = new WorkProcessor( factory, repository, tokenSource );

                    return comp;

            })
                .As<IWorkProcessor>()
                .SingleInstance()
                .AutoActivate();



            if( !string.IsNullOrWhiteSpace(WorkQueueName) )
            {

                builder.Register(c =>
                    {

                        var queue = c.Resolve<IQueueComponent>();
                        var parser = new WorkMessageBodyParser();
                        var processor = c.Resolve<IWorkProcessor>();


                        var comp = new QueueWorkListener(queue, parser, processor)
                        {
                            QueueName = WorkQueueName,
                            PollingDuration = TimeSpan.FromSeconds(PollingDurationSecs),
                            AcknowledgementTimeout = TimeSpan.FromSeconds(AcknowledgementTimeoutSecs)
                        };

                        return comp;

                    })
                    .As<IStartable>()
                    .SingleInstance()
                    .AutoActivate();

            }


            if( !string.IsNullOrWhiteSpace(S3EventQueueName) )
            {

                builder.Register(c =>
                    {

                        var queue = c.Resolve<IQueueComponent>();
                        var parser = new S3EventMessageBodyParser();
                        var processor = c.Resolve<IWorkProcessor>();

                        var comp = new QueueWorkListener(queue, parser, processor)
                        {
                            QueueName = S3EventQueueName,
                            PollingDuration = TimeSpan.FromSeconds(PollingDurationSecs),
                            AcknowledgementTimeout = TimeSpan.FromSeconds(AcknowledgementTimeoutSecs)
                        };

                        return comp;

                    })
                    .As<IStartable>()
                    .SingleInstance()
                    .AutoActivate();

            }



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




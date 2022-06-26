using System;
using System.Drawing;
using System.Net.Http;
using Amazon.SQS;
using Autofac;
using Fabrica.Api.Support.Conventions;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Aws;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.Models.Serialization;
using Fabrica.One.Persistence;
using Fabrica.One.Persistence.Work;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Work.Processor;
using Fabrica.Work.Processor.Parsers;
using Fabrica.Work.Queue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Fabrica.Work.Appliance;

public class TheBootstrap: BaseBootstrap, IAwsCredentialModule, IWorkModule, IOnePersistenceModule
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

    public string IdentitySubject { get; set; } = "";
    public string IdentityName { get; set; } = "";


    public override void ConfigureWatch()
    {

#if DEBUG
        ConfigureDebugWatch(s =>
        {
            s
                .WhenMatched("Fabrica.Diagnostics.Http", "", Level.Debug, Color.Thistle)
                .WhenMatched("Fabrica.Fake", "", Level.Debug, Color.LightSalmon)
                .WhenMatched("Microsoft", "", Level.Warning, Color.BurlyWood)
                .WhenNotMatched(Level.Warning, Color.Azure);
        });
#else
        base.ConfigureWatch();
#endif

    }




    public override void ConfigureServices(IServiceCollection services)
    {

        services.AddMvc(builder =>
            {
#if DEBUG

#else
                builder.Conventions.Add(new DefaultAuthorizeConvention<TokenAuthorizationFilter>());
#endif
                builder.Filters.Add(typeof(ExceptionFilter));
                builder.Filters.Add(typeof(ResultFilter));
            })
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = new ModelContractResolver();
                opt.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            })
            .AddApplicationPart(GetType().Assembly);


        services.Configure<ForwardedHeadersOptions>(options =>
        {

            options.RequireHeaderSymmetry = false;
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

        });

#if DEBUG

#else
            services.AddProxyTokenAuthentication();

            services.AddAuthorization(op =>
            {
                op.AddPolicy(TokenConstants.Policy, b => b.RequireAuthenticatedUser());
            });
#endif


    }

    public override void ConfigureContainer(ContainerBuilder builder)
    {

        using var logger = this.EnterMethod();

        logger.LogObject("TheBootstrap", this);


        builder.UseOnePersitence(OneStoreUri, OneDatabase);

        builder.UseAws(this);

        builder.Register(c =>
        {

            var sqs = c.Resolve<IAmazonSQS>();

            var comp = new SqsQueueComponent(sqs);

            return comp;

        })
            .As<IQueueComponent>()
            .SingleInstance();



        builder.AddHttpClient("WebhookEndpoint", WebhookEndpoint);

        builder.AddProxyTokenEncoder(TokenSigningKey);

        builder.Register(c =>
        {

            var claims = new ClaimSetModel
            {
                Subject = IdentitySubject,
                Name = IdentityName
            };

            var encoder = c.Resolve<IProxyTokenEncoder>();
            var token = encoder.Encode(claims);

            var comp = new StaticAccessTokenSource("Api",token);

            return comp;

        })
            .As<IAccessTokenSource>()
            .SingleInstance();



        builder.Register(c =>
        {

            var factory = c.Resolve<IHttpClientFactory>();
            var repository = c.Resolve<WorkRepository>();
            var tokenSource = c.Resolve<IAccessTokenSource>();

            var comp = new WorkProcessor(factory, repository, tokenSource);

            return comp;

        })
            .As<IWorkProcessor>()
            .SingleInstance()
            .AutoActivate();



        if (!string.IsNullOrWhiteSpace(WorkQueueName))
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
                .As<IRequiresStart>()
                .SingleInstance()
                .AutoActivate();

        }


        if (!string.IsNullOrWhiteSpace(S3EventQueueName))
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
                .As<IRequiresStart>()
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


    public override void ConfigureWebApp( WebApplication app)
    {

        app.UsePipelineMonitor();
        app.UseDebugMode();

        app.UseRequestLogging();

        app.UseForwardedHeaders();

        app.UseRouting();

#if DEBUG

#else
            app.UseAuthentication();
            app.UseAuthorization();
#endif

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


    }



}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.SQS;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Api.Support.Conventions;
using Fabrica.Api.Support.Endpoints;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Api.Support.Swagger;
using Fabrica.Aws;
using Fabrica.Aws.Secrets;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Persistence;
using Fabrica.Persistence.Connection;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Work.Mediator.Handlers;
using Fabrica.Work.Persistence.Contexts;
using Fabrica.Work.Processor;
using Fabrica.Work.Processor.Parsers;
using Fabrica.Work.Queue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Newtonsoft.Json;
using SmartFormat;

namespace Fabrica.Work.Appliance;

public class TheBootstrap : BaseBootstrap, IAwsCredentialModule, IWorkModule
{


    public string Profile { get; set; } = "";
    public string RegionName { get; set; } = "";

    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";

    public bool RunningOnEC2 { get; set; } = true;


    public string WorkQueueName { get; set; } = "";
    public string S3EventQueueName { get; set; } = "";

    public int PollingDurationSecs { get; set; } = 20;
    public int AcknowledgementTimeoutSecs { get; set; } = 30;

    public string WebhookEndpoint { get; set; } = "http://localhost:8080";

    public string IdentitySubject { get; set; } = "";

    public string IdentityName { get; set; } = "";



    public string OriginDbConnectionTemplate { get; set; } = "";
    [Sensitive]
    private string OriginDbConnectionString { get; set; } = "";

    public string ReplicaDbConnectionTemplate { get; set; } = "";
    [Sensitive]
    private string ReplicaDbConnectionString { get; set; } = "";


    public string AwsSecretsId { get; set; } = "";
    private SecretsModel Secrets { get; } = new();


    public override async Task OnConfigured()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Inspect(nameof(AwsSecretsId), AwsSecretsId);
        if (!string.IsNullOrWhiteSpace(AwsSecretsId))
        {

            logger.Debug("Attempting to populate Secrets from AWS Secrets");

            await AwsSecretsHelper.PopulateWithSecrets(Secrets, AwsSecretsId, RunningOnEC2, Profile, RegionName);

            logger.LogObject(nameof(Secrets), Secrets);

            OriginDbConnectionString = Smart.Format(OriginDbConnectionTemplate, Secrets);
            ReplicaDbConnectionString = Smart.Format(ReplicaDbConnectionTemplate, Secrets);

        }
        else
        {
            OriginDbConnectionString = OriginDbConnectionTemplate;
            ReplicaDbConnectionString = ReplicaDbConnectionTemplate;
        }


    }



    public override void ConfigureServices(IServiceCollection services)
    {


        services.AddMvcCore(builder =>
            {

                if (RequiresAuthentication)
                    builder.Conventions.Add(new DefaultAuthorizeConvention<TokenAuthorizationFilter>());

                builder.Filters.Add(typeof(ExceptionFilter));
                builder.Filters.Add(typeof(ResultFilter));

            })
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = new ModelContractResolver();
                opt.SerializerSettings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            })
            .AddApplicationPart(GetType().Assembly)
            .AddApiExplorer()
            .AddAuthorization()
            .AddFormatterMappings()
            .AddDataAnnotations();



        services.Configure<ForwardedHeadersOptions>(options =>
        {

            options.RequireHeaderSymmetry = false;
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

        });


        services.AddProxyTokenAuthentication();

        services.AddAuthorization(op =>
        {
            op.AddPolicy(TokenConstants.Policy, b => b.RequireAuthenticatedUser());
        });



        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("data", new OpenApiInfo { Title = "Fabrica Work API", Version = "v1" });
            c.EnableAnnotations();
            c.SchemaFilter<NoAdditionalPropertiesFilter>();

            c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    return new[] { controllerActionDescriptor.ControllerName };
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            c.OrderActionsBy((api) => $"{api.GroupName ?? ""}_{api.HttpMethod}");

            c.DocInclusionPredicate((_, _) => true);

        });

        services.AddSwaggerGenNewtonsoftSupport();


    }

    public override void ConfigureContainer(ContainerBuilder builder)
    {

        using var logger = EnterMethod();

        logger.LogObject("TheBootstrap", this);


        builder.UseRules();
        builder.AddRules(GetType().Assembly);

        builder.UseMediator(typeof(QueryWorkTopicHandler).Assembly);

        builder.AddAuditJournalHandler();

        builder.RegisterAutoMapper(GetType().Assembly);

        builder.UseModelMeta()
            .AddModelMetaSource(GetType().Assembly);

        builder.AddEndpointComponent();


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var meta = c.Resolve<IModelMetaService>();
            var mediator = c.Resolve<IMessageMediator>();
            var factory = c.Resolve<IMediatorRequestFactory>();

            var comp = new PatchResolver(corr, meta, mediator, factory);
            return comp;

        })
            .AsSelf()
            .As<IPatchResolver>()
            .InstancePerLifetimeScope();


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var comp = new MediatorRequestFactory(corr);

            return comp;

        })
            .As<IMediatorRequestFactory>()
            .SingleInstance();



        builder.UsePersistence()
            .AddSingleTenantResolver(MySqlConnectorFactory.Instance, ReplicaDbConnectionString, OriginDbConnectionString);


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var factory = c.Resolve<ILoggerFactory>();
            var resolver = c.Resolve<IConnectionResolver>();

            var ob = new DbContextOptionsBuilder();
            ob.UseMySql(resolver.ReplicaConnectionStr, ServerVersion.AutoDetect(resolver.ReplicaConnectionStr));

            var ctx = new ExplorerDbContext(corr, ob.Options, factory);

            return ctx;

        })
            .AsSelf()
            .As<ReplicaDbContext>()
            .InstancePerLifetimeScope();


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var resolver = c.Resolve<IConnectionResolver>();
            var uow = c.Resolve<IUnitOfWork>();
            var rules = c.Resolve<IRuleSet>();
            var factory = c.Resolve<ILoggerFactory>();


            var ob = new DbContextOptionsBuilder();
            ob.UseMySql(uow.OriginConnection, ServerVersion.AutoDetect(resolver.OriginConnectionStr));

            var ctx = new WorkDbContext(corr, rules, ob.Options, factory);
            ctx.Database.UseTransaction(uow.Transaction);

            return ctx;

        })
            .AsSelf()
            .InstancePerLifetimeScope();






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

            var comp = new StaticAccessTokenSource("Api", token);

            return comp;

        })
            .As<IAccessTokenSource>()
            .SingleInstance();



        builder.Register(c =>
        {

            var factory = c.Resolve<IHttpClientFactory>();
            var context = c.Resolve<WorkDbContext>();
            var tokenSource = c.Resolve<IAccessTokenSource>();

            var comp = new WorkProcessor(factory, context, tokenSource);

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

                    var corr = c.Resolve<ICorrelation>();
                    var queue = c.Resolve<IQueueComponent>();

                    var t = new WorkTopicTransformer(corr)
                    {
                        PrefixCount = 2,
                        SuffixCount = 1,
                        Prepend     = "Ingest",
                        Separator   = "",
                        DefaultName = "root"
                    };

                    var parser = new S3EventMessageBodyParser(t);
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


    public override void ConfigureWebApp(WebApplication app)
    {

        app.UsePipelineMonitor();
        app.UseDebugMode();

        app.UseRequestLogging();

        app.UseForwardedHeaders();


        app.UseSwagger(o =>
        {

            o.PreSerializeFilters.Add((d, r) =>
            {
                var url = $"https://{r.Host}";
                d.Servers = new List<OpenApiServer> { new() { Url = url } };
            });


        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("data/swagger.json", "Fabrica Work API");
        });


        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


    }



}


public class SecretsModel
{

    [JsonProperty("origin-db-user-name")]
    public string OriginDbUserName { get; set; } = "";
    [Sensitive]
    [JsonProperty("origin-db-password")]
    public string OriginDbPassword { get; set; } = "";

    [JsonProperty("replica-db-user-name")]
    public string ReplicaDbUserName { get; set; } = "";
    [Sensitive]
    [JsonProperty("replica-db-password")]
    public string ReplicaDbPassword { get; set; } = "";

}



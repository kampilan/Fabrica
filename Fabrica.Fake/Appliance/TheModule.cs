using System.Reflection;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Api.Support.Conventions;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Aws;
using Fabrica.Fake.Persistence;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fabrica.Fake.Appliance;

public class TheModule: BootstrapModule, IAwsCredentialModule
{


    public string Profile { get; set; } = "";
    public string RegionName { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public bool RunningOnEC2 { get; set; } = true;


    public string TokenSigningKey { get; set; }

    public int PersonCount { get; set; } = 1000;
    public int CompanyCount { get; set; } = 1000;


    private static InMemoryDatabaseRoot _root = new();


    public override void ConfigureServices(IServiceCollection services)
    {

        using var logger = this.EnterMethod();


        services.AddHostedService<FakeInitService>();

        services.AddMvc(builder =>
            {


                if( RequiresAuthentication )
                {
                    builder.Conventions.Add(new DefaultAuthorizeConvention<TokenAuthorizationFilter>());
                }

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
            });


        services.Configure<ForwardedHeadersOptions>(options =>
        {

            options.RequireHeaderSymmetry = false;
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

        });


        if( RequiresAuthentication )
        {
            services.AddProxyTokenAuthentication();
            services.AddProxyTokenAuthorization();
        }




    }


    public override void ConfigureContainer(ContainerBuilder builder)
    {

        using var logger = this.EnterMethod();


        builder.AddCorrelation();

        builder.UseAws(this);

        builder.UseRules();

        builder.RegisterAutoMapper();

        builder.UseModelMeta()
            .AddModelMetaSource(Assembly.GetExecutingAssembly());

        builder.UseMediator(Assembly.GetExecutingAssembly());


        if (!string.IsNullOrWhiteSpace(TokenSigningKey))
            builder.AddProxyTokenEncoder(TokenSigningKey);


        builder.RegisterType<InMemoryUnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var rules = c.Resolve<IRuleSet>();
            var factory = c.ResolveOptional<ILoggerFactory>();


            var ob = new DbContextOptionsBuilder();
            ob.UseInMemoryDatabase("Faker", _root);

            var ctx = new FakeOriginDbContext(corr, rules, ob.Options, factory);

            return ctx;

        })
            .AsSelf()
            .InstancePerLifetimeScope();


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var factory = c.ResolveOptional<ILoggerFactory>();

            var ob = new DbContextOptionsBuilder();
            ob.UseInMemoryDatabase("Faker", _root);

            var ctx = new FakeReplicaDbContext(corr, ob.Options, factory);

            return ctx;

        })
            .AsSelf()
            .InstancePerLifetimeScope();


        builder.Register(c =>
        {

            var corr = c.Resolve<ICorrelation>();
            var comp = new MediatorRequestFactory(corr);

            return comp;

        })
            .As<IMediatorRequestFactory>()
            .SingleInstance();


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


    }


    public override void ConfigureWebApp(IApplicationBuilder builder)
    {

        using var logger = this.EnterMethod();


        builder.UsePipelineMonitor();
        builder.UseDebugMode();

        builder.UseRequestLogging();

        builder.UseForwardedHeaders();

        builder.UseRouting();

        if( RequiresAuthentication )
        {
            builder.UseAuthentication();
            builder.UseAuthorization();
        }



        builder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


    }




}
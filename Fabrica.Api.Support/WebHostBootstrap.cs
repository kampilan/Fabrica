


using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Api.Support.Endpoints;
using Fabrica.Api.Support.Identity.Gateway;
using Fabrica.Api.Support.Swagger;
using Fabrica.Container;
using Fabrica.Models.Serialization;
using Fabrica.One;
using Fabrica.Services;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Bridges.MicrosoftImpl;
using Fabrica.Watch.Switching;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support;

public abstract class WebHostBootstrap() : CorrelatedObject(new Correlation()), IBootstrap
{


    public bool AllowManualExit { get; set; } = false;

    public string ApplicationLifetimeType { get; set; } = "FabricaOne";

    
    public bool QuietLogging { get; set; } = false;

    public bool RealtimeLogging { get; set; } = false;
    public List<LocalSwitchConfig> RealtimeSwitches { get; set; } = new();

    public bool RelayLogging { get; set; } = false;


    public string WatchEventStoreUri { get; set; } = string.Empty;
    public string WatchDomainName { get; set; } = string.Empty;
    public int WatchPollingDurationSecs { get; set; } = 15;


    public bool AllowAnyIp { get; set; } = false;
    public int ListeningPort { get; set; } = 8080;


    public string Environment { get; set; } = "Development";
    public string MissionName { get; set; } = string.Empty;
    public bool RunningAsMission => !string.IsNullOrWhiteSpace(MissionName);


    public string ApplianceId { get; set; } = string.Empty;
    public string ApplianceName { get; set; } = string.Empty;
    public string ApplianceBuild { get; set; } = string.Empty;
    public DateTime ApplianceBuildDate { get; set; } = DateTime.MinValue;
    public string ApplianceRoot { get; set; } = string.Empty;
    public DateTime ApplianceStartTime { get; set; } = DateTime.MinValue;


    public bool RequiresAuthentication { get; set; } = true;
    public string GatewayTokenSigningKey { get; set; } = string.Empty;
    public string TokenSigningKey { get; set; } = string.Empty;

    public bool ExposeApiDocumentation { get; set; } = true;
    public string ApiName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = string.Empty;

    public IConfiguration Configuration { get; set; } = null!;


    public virtual void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseQuiet();

        maker.Build();

    }

    protected IHostBuilder Builder { get; set; } = null!;


    public async Task<IAppliance> Boot(string path = "")
    {

        var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to call OnConfigure");
        await OnConfigured();

        logger.LogObject("WebHostBootstrap", this);



        // *****************************************************************
        logger.Debug("Attempting to build WebApplicationBuilder");
        Builder = Host.CreateDefaultBuilder();


        // *****************************************************************
        logger.Debug("Attempting to Add Host Configuration ");
        Builder.ConfigureHostConfiguration(cfb => cfb.AddConfiguration(Configuration));



        // *****************************************************************
        logger.Debug("Attempting to Add App Configuration ");
        Builder.ConfigureAppConfiguration((cfb) => cfb.AddConfiguration(Configuration));



        // *****************************************************************
        logger.Debug("Attempting to Configure the Microsoft Logging bridge");
        Builder.ConfigureLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddProvider(new LoggerProvider());
            lb.SetMinimumLevel(LogLevel.Trace);
        });



        // *****************************************************************
        logger.Debug("Attempting to call ConfigureServices");
        Builder.ConfigureServices(sc =>
        {

            
            sc.AddHostedService<RequiresStartService>();


            sc.Configure<ForwardedHeadersOptions>(options =>
            {

                options.RequireHeaderSymmetry = false;
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

            });


            sc.AddRouting();


            if ( RequiresAuthentication )
            {

                var token = !string.IsNullOrWhiteSpace(GatewayTokenSigningKey) ? GatewayTokenSigningKey : TokenSigningKey;
                sc.AddGatewayTokenAuthentication(token);


                sc.AddAuthorization(op =>
                {

                    op.FallbackPolicy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(IdentityConstants.Scheme)
                        .RequireAuthenticatedUser()
                        .Build();

                    op.AddPolicy(ModuleConstants.PublicPolicyName, b => b.RequireAssertion(_ => true));

                    op.AddPolicy(ModuleConstants.AdminPolicyName, b =>
                    {
                        b.AddAuthenticationSchemes(IdentityConstants.Scheme)
                            .RequireAuthenticatedUser()
                            .RequireRole(ModuleConstants.AdminRoleName);
                    });


                });

            }

            if( ExposeApiDocumentation && !string.IsNullOrWhiteSpace(ApiName) )
            {

                sc.AddEndpointsApiExplorer();
                sc.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("docs", new OpenApiInfo { Title = ApiName, Version = ApiVersion });
                    c.EnableAnnotations();
                    c.SchemaFilter<NoAdditionalPropertiesFilter>();
                });

                sc.AddModelContractSwaggerGenSupport();

            }


        });



        // *****************************************************************
        logger.Debug("Attempting to call UseServiceProviderFactory");
        Builder.UseServiceProviderFactory(new FabricaServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(Configuration)
                .As<IConfiguration>()
                .SingleInstance();


            cb.AddCorrelation();


            var mc = Configuration.Get<MissionContext>();
            if( mc is not null )
            {

                cb.RegisterInstance(mc)
                    .AsSelf()
                    .As<IMissionContext>()
                    .SingleInstance();

                foreach (var pair in mc.ServiceEndpoints)
                {
                    var address = pair.Value.EndsWith("/") ? pair.Value : $"{pair.Value}/";
                    cb.AddServiceAddress(pair.Key, address);
                }

            }



            using var inner = GetLogger();

            try
            {

                var services = new ServiceCollection();
                
                inner.Debug("Attempting to call ConfigureContainer");
                BuildHost(Builder, services, cb);

                cb.Populate(services);

            }
            catch (Exception cause)
            {
                inner.ErrorWithContext(cause, this, "Bootstrap ConfigureContainer failed.");
                throw;
            }


        }));



        // *****************************************************************
        logger.Debug("Attempting to Configure WebHost");
        Builder.ConfigureWebHost(whb =>
        {


            whb.UseKestrel(op =>
            {

                if (AllowAnyIp)
                    op.ListenAnyIP(ListeningPort);
                else
                    op.ListenLocalhost(ListeningPort);

            });


            whb.Configure(app =>
            {

                using var inner = GetLogger();

                try
                {

                    inner.Debug("Attempting to call BuildWebApp");

                    BuildWebApp(whb, app);

                }
                catch (Exception cause)
                {
                    inner.ErrorWithContext(cause, this, "Bootstrap BuildWebApp failed.");
                    throw;
                }

            });


        });




        // *****************************************************************
        logger.Debug("Attempting to build Host");
        var host = Builder.Build();



        // *****************************************************************
        logger.Debug("Attempting to create HostAppliance");
        var app = new HostAppliance(host);



        // *****************************************************************
        return app;



    }


    public virtual Task OnConfigured()
    {

        using var logger = EnterMethod();

        logger.Info("Base OnConfigured does nothing");

        return Task.CompletedTask;

    }


    public virtual void BuildHost(IHostBuilder host, IServiceCollection services, ContainerBuilder builder)
    {

        using var logger = EnterMethod();

        logger.Info("Base Build does nothing");


    }


    public virtual void BuildWebApp(IWebHostBuilder host, IApplicationBuilder app)
    {

        using var logger = EnterMethod();

        logger.Info("Base Build does nothing");


    }



}


public static class NewtonsoftServiceCollectionExtensions
{

    public static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new ModelContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        DefaultValueHandling = DefaultValueHandling.Populate,
        NullValueHandling = NullValueHandling.Ignore
    };

    public static IServiceCollection AddModelContractSwaggerGenSupport(this IServiceCollection services)
    {
        return services.Replace(ServiceDescriptor.Transient<ISerializerDataContractResolver>(_ => new NewtonsoftDataContractResolver(Settings)));
    }

}


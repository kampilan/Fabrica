
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Api.Support.Handlers;
using Fabrica.Container;
using Fabrica.One;
using Fabrica.One.Appliance;
using Fabrica.Services;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Bridges.MicrosoftImpl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fabrica.Api.Support;

public abstract class WebApplicationBootstrap() : CorrelatedObject(new Correlation()), IBootstrap
{


    public bool AllowManualExit { get; set; } = false;

    public string ApplicationLifetimeType { get; set; } = "FabricaOne";

    
    public bool QuietLogging { get; set; } = false;

    public bool RealtimeLogging { get; set; } = false;
    public List<LocalSwitchConfig> RealtimeSwitches { get; set; } = new();

    public bool RelayLogging { get; set; } = false;


    public string WatchEventStoreUri { get; set; } = "";
    public string WatchDomainName { get; set; } = "";
    public int WatchPollingDurationSecs { get; set; } = 15;


    public bool AllowAnyIp { get; set; } = false;
    public int ListeningPort { get; set; } = 8080;


    public string Environment { get; set; } = "Development";
    public string MissionName { get; set; } = "";
    public bool RunningAsMission => !string.IsNullOrWhiteSpace(MissionName);


    public string ApplianceId { get; set; } = "";
    public string ApplianceName { get; set; } = "";
    public string ApplianceBuild { get; set; } = "";
    public DateTime ApplianceBuildDate { get; set; } = DateTime.MinValue;
    public string ApplianceRoot { get; set; } = "";
    public DateTime ApplianceStartTime { get; set; } = DateTime.MinValue;


    public bool RequiresAuthentication { get; set; } = true;
    public string GatewayTokenSigningKey { get; set; } = "";
    public string TokenSigningKey { get; set; } = "";

    public bool ExposeApiDocumentation { get; set; } = true;

    public IConfiguration Configuration { get; set; } = null!;


    public virtual void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseQuiet();

        maker.Build();

    }


    public async Task<IAppliance> Boot<TService>(string path = "") where TService : class, IHostedService
    {

        var logger = EnterMethod();

        logger.Inspect("Service Type", typeof(TService).FullName);



        // *****************************************************************
        logger.Debug("Attempting to call OnConfigure");
        await OnConfigured();

        logger.LogObject("WebApplicationBootstrap", this);



        // *****************************************************************
        logger.Debug("Attempting to build WebApplicationBuilder");
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddConfiguration(Configuration);



        // *****************************************************************
        logger.Debug("Attempting to Configure the Microsoft Logging bridge");
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new LoggerProvider());
        builder.Logging.SetMinimumLevel(LogLevel.Trace);



        // *****************************************************************
        if( ApplicationLifetimeType.ToLowerInvariant() == "fabricaone" )
        {
            logger.Debug("Attempting to add FabricaOne application lifetime");
            builder.Host.UseFabricaOne(path, AllowManualExit);
        }
        else if (ApplicationLifetimeType.ToLowerInvariant() == "systemd")
        {
            logger.Debug("Attempting to add FabricaOne application lifetime");
            builder.Host.UseSystemd();
        }



        // *****************************************************************
        logger.Debug("Attempting to configure Host");
        try
        {
            ConfigureHost(builder.Host);
        }
        catch (Exception cause)
        {
            logger.ErrorWithContext(cause, this, "Bootstrap ConfigureHost failed.");
            throw;
        }


        try
        {
            ConfigureServices(builder.Services);
        }
        catch (Exception cause)
        {
            logger.ErrorWithContext(cause, this, "Bootstrap ConfigureServices failed.");
            throw;
        }

        builder.Services.AddHostedService<TService>();



        // *****************************************************************
        logger.Debug("Attempting to configure Autofac container");
        builder.Host.UseServiceProviderFactory(new FabricaServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(Configuration)
                .As<IConfiguration>()
                .SingleInstance();


            cb.AddCorrelation();


            cb.RegisterType<CircuitBootstrap>()
                .As<CircuitHandler>()
                .InstancePerLifetimeScope();


            cb.Register(c =>
                {

                    var scope = c.Resolve<ILifetimeScope>();
                    var comp = new FabricaServiceScopeFactory(scope, ConfigureScopedContainer);

                    return comp;

                })
                .AsSelf()
                .As<IServiceScopeFactory>()
                .SingleInstance()
                .AutoActivate();



            var mc = Configuration.Get<MissionContext>();
            if( mc is not null )
            {
                foreach (var pair in mc.ServiceEndpoints)
                {
                    var address = pair.Value.EndsWith("/") ? pair.Value : $"{pair.Value}/";
                    cb.AddServiceAddress(pair.Key, address);
                }
            }


            using var inner = GetLogger();

            try
            {
                ConfigureContainer(cb);
            }
            catch (Exception cause)
            {
                inner.ErrorWithContext(cause, this, "Bootstrap ConfigureContainer failed.");
                throw;
            }


        }));



        // *****************************************************************
        logger.Debug("Attempting to configure Kestrel");
        builder.WebHost.UseKestrel(op =>
        {

            if (AllowAnyIp)
                op.ListenAnyIP(ListeningPort);
            else
                op.ListenLocalhost(ListeningPort);

        });



        // *****************************************************************
        logger.Debug("Attempting to configure web app");
        var app = builder.Build();

        try
        {
            ConfigureWebApp(app);
        }
        catch (Exception cause)
        {
            logger.ErrorWithContext(cause, this, "Bootstrap ConfigureWebApp failed.");
            throw;
        }


        var appState = new
        {
            Environment = app.Environment.EnvironmentName,
            WebPootPath = app.Environment.WebRootPath,
            ContentPath = app.Environment.ContentRootPath,
            Urls = string.Join(',', app.Urls)
        };

        logger.LogObject(nameof(appState), appState);



        // *****************************************************************
        return new WebAppliance(app);


    }


    public virtual Task OnConfigured()
    {

        using var logger = EnterMethod();

        logger.Info("Base OnConfigured does nothing");

        return Task.CompletedTask;

    }


    public virtual void ConfigureHost(IHostBuilder builder)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureHost does nothing");


    }


    public virtual void ConfigureServices(IServiceCollection services)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureServices adds no services");


    }


    public virtual void ConfigureContainer(ContainerBuilder builder)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureContainer adds no services");

    }

    public virtual void ConfigureScopedContainer(ContainerBuilder builder)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureScopedContainer adds no services");

    }



    public virtual void ConfigureWebApp(WebApplication app)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureWebApp does nothing");

    }


}

public class WebAppliance(WebApplication app) : IAppliance
{
    private WebApplication App { get; } = app;

    public void Run()
    {
        App.Run();
    }

    public async Task RunAsync()
    {
        await App.RunAsync();
    }

}


public class LocalSwitchConfig
{
    public string Pattern { get; set; } = "";
    public string Level { get; set; } = "";
    public string Color { get; set; } = "";

}
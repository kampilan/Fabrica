using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Process;
using Fabrica.Watch;
using Fabrica.Watch.Bridges.MicrosoftImpl;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Realtime;
using Fabrica.Watch.Switching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.One;

public abstract class BootstrapModule: CorrelatedObject 
{


    public bool RealtimeLogging { get; set; } = false;

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
    public string ApplianceRoot { get; set; } = "";

    public bool RequiresAuthentication { get; set; } = true;
    public string TokenSigningKey { get; set; } = "";


    public IConfiguration Configuration { get; set; }


    protected BootstrapModule(): base(new Correlation())
    {
    }

    public virtual void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();
        if (RealtimeLogging || string.IsNullOrWhiteSpace(WatchDomainName) || string.IsNullOrWhiteSpace(WatchEventStoreUri))
            maker.UseRealtime(Level.Debug, Color.LightPink);
        else
            maker.UseMongo(WatchEventStoreUri, WatchDomainName);

        maker.Build();

    }

    protected void ConfigureDebugWatch( Action<SwitchSource> switchBuilder = null )
    {

        var maker = WatchFactoryBuilder.Create();
        maker.UseRealtime();

        if (!(switchBuilder is null))
        {
            var switches = maker.UseLocalSwitchSource();
            switchBuilder(switches);
        }
        else
        {
            maker.UseLocalSwitchSource()
                .WhenNotMatched(Level.Debug, Color.Azure);
        }

        maker.Build();

    }

    public async Task<WebApplication> Boot<TService>() where TService: InitService
    {

        var logger = EnterMethod();

        logger.Inspect("Service Type", typeof(TService).FullName);



        // *****************************************************************
        logger.Debug("Attempting to call OnConfigure");
        await OnConfigured();

        logger.LogObject("BootstrapModule", this);



        // *****************************************************************
        logger.Debug("Attempting to build WebApplicationBuilder");
        var wao = new WebApplicationOptions
        {
            ContentRootPath = ApplianceRoot,
            WebRootPath = $"{ApplianceRoot}{Path.DirectorySeparatorChar}wwwroot"
        };

        var builder = WebApplication.CreateBuilder(wao);

        builder.Configuration.AddConfiguration(Configuration);



        // *****************************************************************
        logger.Debug("Attempting to Configure the Microsoft logging bridge");
        builder.Host.ConfigureLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddProvider(new LoggerProvider());
            lb.SetMinimumLevel(LogLevel.Trace);
        });



        // *****************************************************************
        logger.Debug("Attempting to configure services");
        builder.Host.ConfigureServices(s =>
        {

            s.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>();

            using var inner = GetLogger();

            try
            {
                ConfigureServices(s);
            }
            catch (Exception cause)
            {
                inner.ErrorWithContext(cause, this, "Bootstrap ConfigureServices failed.");
                throw;
            }

            s.AddHostedService<TService>();

        });



        // *****************************************************************
        logger.Debug("Attempting to configure Autofac container");
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(Configuration)
                .As<IConfiguration>()
                .SingleInstance();

            cb.Register(_ =>
                {

                    var comp = new FileSignalController(FileSignalController.OwnerType.Appliance);
                    return comp;

                })
                .As<ISignalController>()
                .As<IRequiresStart>()
                .SingleInstance()
                .AutoActivate();

            cb.Register(c =>
                {

                    var hal = c.Resolve<IHostApplicationLifetime>();
                    var sc = c.Resolve<ISignalController>();

                    var comp = new ApplianceLifetime(hal, sc);

                    return comp;

                })
                .AsSelf()
                .As<IRequiresStart>()
                .SingleInstance()
                .AutoActivate();


            cb.AddCorrelation();


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
        return app;


    }


    public virtual Task OnConfigured()
    {

        using var logger = EnterMethod();

        logger.Info("Base OnConfigured does nothing");
        
        return Task.CompletedTask;

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


    public virtual void ConfigureWebApp(IApplicationBuilder app)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureWebApp does nothing");

    }

    public virtual void ConfigureWebApp( WebApplication app )
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureWebApp does nothing");

    }


}
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Configuration.Yaml;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Process;
using Fabrica.Watch;
using Fabrica.Watch.Bridges.MicrosoftImpl;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Fabrica.Watch.ILogger;

// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.One;

public static class OneAppliance
{

    public static WebApplicationBuilder AttachLifetime(this WebApplicationBuilder builder)
    {

        // *****************************************************************
        builder.Host.ConfigureServices((_, collection) => collection.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>());

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(cb =>
        {

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

        }))
            .ConfigureServices(s =>
            {
                s.AddHostedService<InitService>();
            });


        return builder;

    }

    public static async Task<WebApplication> Bootstrap<TModule>() where TModule : BootstrapModule
    {

        var app = await Bootstrap<TModule, InitService>();

        return app;

    }    

    public static async Task<WebApplication> Bootstrap<TModule,TService>() where TModule : BootstrapModule where TService: class, IHostedService
    {

        static ILogger GetLogger()
        {
            return WatchFactoryLocator.Factory.GetLogger("Fabrica.Bootstrap");
        }


        // *****************************************************************
        var cfgb = new ConfigurationBuilder();

        cfgb
            .AddYamlFile("configuration.yml", true)
            .AddJsonFile("environment.json", true)
            .AddJsonFile("mission.json", true);


        var configuration = cfgb.Build();




        // *****************************************************************
        var options = configuration.Get<WatchMongoOptions>();
        var maker = WatchFactoryBuilder.Create();
        if (options == null || options.RealtimeLogging || string.IsNullOrWhiteSpace(options.WatchDomainName) || string.IsNullOrWhiteSpace(options.WatchEventStoreUri))
            maker.UseRealtime(Level.Debug, Color.LightPink);
        else
            maker.UseMongo(options);

        maker.Build();

        using var outerLogger = WatchFactoryLocator.Factory.GetLogger("Fabrica.Bootstrap");



        // *****************************************************************
        outerLogger.Debug("Attempting to build BootstrapModule");
        var bootstrap = configuration.Get<TModule>();
        bootstrap.Configuration = configuration;

        outerLogger.LogObject(nameof(bootstrap), bootstrap);

        try
        {
            await bootstrap.OnConfigured();
        }
        catch (Exception cause)
        {
            outerLogger.ErrorWithContext(cause, bootstrap, "Bootstrap OnConfigure failed.");
            throw;
        }



        // *****************************************************************
        outerLogger.Debug("Attempting to configure and create WebApplicationBuilder");
        var wao = new WebApplicationOptions
        {
            ContentRootPath = bootstrap.ApplianceRoot,
            WebRootPath     = $"{bootstrap.ApplianceRoot}{Path.DirectorySeparatorChar}wwwroot",
            ApplicationName = bootstrap.ApplianceName,
            EnvironmentName = "Production"
        };

        var builder = WebApplication.CreateBuilder(wao);

        builder.Configuration.AddConfiguration(configuration);



        // *****************************************************************
        outerLogger.Debug("Attempting to Configure the Watch Microsoft logging bridge");
        builder.Host.ConfigureLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddProvider(new LoggerProvider());
            lb.SetMinimumLevel(LogLevel.Trace);

        });




        // *****************************************************************
        outerLogger.Debug("Attempting to configure services");
        builder.Host.ConfigureServices(s =>
        {

            s.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>();

            using var logger = GetLogger();

            try
            {
                bootstrap.ConfigureServices(s);
            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, bootstrap, "Bootstrap ConfigureServices failed.");
                throw;
            }

            s.AddHostedService<TService>();

        });



        // *****************************************************************
        outerLogger.Debug("Attempting to configure Autofac container");
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(configuration)
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


            using var logger = GetLogger();

            try
            {
                bootstrap.ConfigureContainer(cb);
            }
            catch (Exception cause)
            {
                logger.ErrorWithContext(cause, bootstrap, "Bootstrap ConfigureContainer failed.");
                throw;
            }


        }));



        // *****************************************************************
        outerLogger.Debug("Attempting to configure Kestrel");
        builder.WebHost.UseKestrel(op =>
        {

            if( bootstrap.AllowAnyIp )
                op.ListenAnyIP(bootstrap.ListeningPort);
            else
                op.ListenLocalhost(bootstrap.ListeningPort);

        });



        // *****************************************************************
        outerLogger.Debug("Attempting to set ContentRoot" );
        try
        {

            outerLogger.Debug("Attempting to set WebRootPath on WebHost");
            var webRoot = $"{bootstrap.ApplianceRoot}{Path.DirectorySeparatorChar}wwwroot";
            outerLogger.Inspect(nameof(webRoot), webRoot);

            builder.WebHost.UseWebRoot(webRoot);

        }
        catch (Exception cause)
        {
            outerLogger.ErrorWithContext(cause, bootstrap, "Bootstrap set web root failed.");
            throw;
        }



        // *****************************************************************
        outerLogger.Debug("Attempting to configure web app");
        var app = builder.Build();

        try
        {
            bootstrap.ConfigureWebApp(app);
        }
        catch (Exception cause)
        {
            outerLogger.ErrorWithContext(cause, bootstrap, "Bootstrap ConfigureWebApp failed.");
            throw;
        }


        var appState = new
        {
            Environment = app.Environment.EnvironmentName,
            WebPootPath = app.Environment.WebRootPath,
            ContentPath = app.Environment.ContentRootPath,
            Urls        = string.Join(',', app.Urls)
        };

        outerLogger.LogObject(nameof(appState), appState);



        // *****************************************************************
        return app;

    }



}
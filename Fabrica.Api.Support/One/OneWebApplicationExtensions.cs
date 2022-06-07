using System;
using System.Drawing;
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
using Fabrica.Watch.Switching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fabrica.Api.Support.One;

public static class OneWebApplicationExtensions
{


    public static WebApplicationBuilder AttachApplianceLifetime(this WebApplicationBuilder builder)
    {

        // *****************************************************************
        builder.Host.ConfigureServices((context, collection) => collection.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>());

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

    public static WebApplicationBuilder BootstrapAppliance(this WebApplicationBuilder builder)
    {


        // *****************************************************************
        var cb = new ConfigurationBuilder();

        cb
            .AddYamlFile("configuration.yml", true)
            .AddJsonFile("environment.json", true)
            .AddJsonFile("mission.json", true);


        var configuration = cb.Build();

        builder.Configuration.AddConfiguration(configuration);



        // *****************************************************************
        var options = configuration.Get<WatchMongoOptions>();
        var maker = WatchFactoryBuilder.Create();
        if (options == null || options.RealtimeLogging || string.IsNullOrWhiteSpace(options.WatchDomainName) || string.IsNullOrWhiteSpace(options.WatchEventStoreUri))
            maker.UseRealtime(Level.Debug, Color.LightPink);
        else
            maker.UseMongo(options);

        maker.Build();

        // *****************************************************************
        builder.Host.ConfigureServices((context, collection) => collection.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>());

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

        }))
            .ConfigureLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddProvider(new LoggerProvider());
                lb.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices(s =>
            {
                s.AddHostedService<InitService>();
            });


        return builder;

    }

    public static async Task<WebApplication> BootstrapAppliance<TModule>(this WebApplicationBuilder builder) where TModule : BootstrapModule
    {

        var app = await BootstrapAppliance<TModule, InitService>(builder);

        return app;

    }    

    public static async Task<WebApplication> BootstrapAppliance<TModule,TService>(this WebApplicationBuilder builder) where TModule : BootstrapModule where TService: class, IHostedService
    {


        // *****************************************************************
        var cb = new ConfigurationBuilder();

        cb
            .AddYamlFile("configuration.yml", true)
            .AddJsonFile("environment.json", true)
            .AddJsonFile("mission.json", true);


        var configuration = cb.Build();

        builder.Configuration.AddConfiguration(configuration);



        // *****************************************************************
        var options = configuration.Get<WatchMongoOptions>();
        var maker = WatchFactoryBuilder.Create();
        if (options == null || options.RealtimeLogging || string.IsNullOrWhiteSpace(options.WatchDomainName) || string.IsNullOrWhiteSpace(options.WatchEventStoreUri))
            maker.UseRealtime(Level.Debug, Color.LightPink);
        else
            maker.UseMongo(options);

        maker.Build();

        builder.Host.ConfigureLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddProvider(new LoggerProvider());
            lb.SetMinimumLevel(LogLevel.Trace);

        });


        using var logger = WatchFactoryLocator.Factory.GetLogger("Fabrica.Bootstrap");



        // *****************************************************************
        logger.Debug("Attempting to build BootstrapModule");
        var bootstrap = configuration.Get<TModule>();
        bootstrap.Configuration = configuration;

        logger.LogObject(nameof(bootstrap), bootstrap);

        
        await bootstrap.OnConfigured();



        // *****************************************************************
        logger.Debug("Attempting to configure services");
        builder.Host.ConfigureServices(s =>
        {

            s.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>();

            bootstrap.ConfigureServices(s);

            s.AddHostedService<TService>();

        });



        // *****************************************************************
        logger.Debug("Attempting to configure Autofac container");
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


            bootstrap.ConfigureContainer(cb);


        }));



        // *****************************************************************
        logger.Debug("Attempting to configure Kestrel");
        builder.WebHost.UseKestrel(op =>
        {

            if( bootstrap.AllowAnyIp )
                op.ListenAnyIP(bootstrap.ListeningPort);
            else
                op.ListenLocalhost(bootstrap.ListeningPort);

        });



        // *****************************************************************
        logger.Debug("Attempting to configure web app");
        var app = builder.Build();

        bootstrap.ConfigureWebApp(app);

        var status = new
        {
            Environment = app.Environment.EnvironmentName,
            WebPootPath = app.Environment.WebRootPath,
            ContentPath = app.Environment.ContentRootPath,
            Urls        = string.Join(',', app.Urls),
            bootstrap.ListeningPort,
            bootstrap.AllowAnyIp,
            bootstrap.MissionName,
            bootstrap.RunningAsMission
        };

        logger.LogObject(nameof(status), status);



        // *****************************************************************
        return app;

    }

    public static async  Task<WebApplication> BootstrapDebugAppliance<TModule>(this WebApplicationBuilder builder, string localConfigFile = "", Action<SwitchSource> switchBuilder = null) where TModule : BootstrapModule
    {

        var app = await BootstrapDebugAppliance<TModule,InitService>( builder, localConfigFile, switchBuilder );

        return app;

    }

    public static async Task<WebApplication> BootstrapDebugAppliance<TModule,TService>(this WebApplicationBuilder builder, string localConfigFile = "", Action<SwitchSource> switchBuilder = null) where TModule : BootstrapModule where TService: class, IHostedService
    {


        // *****************************************************************
        var cb = new ConfigurationBuilder();

        cb.AddYamlFile("configuration.yml", true);

        if (!string.IsNullOrWhiteSpace(localConfigFile))
            cb.AddYamlFile(localConfigFile, true);

        var configuration = cb.Build();

        builder.Configuration.AddConfiguration(configuration);



        // *****************************************************************
        var maker = new WatchFactoryBuilder();

        if (switchBuilder is null)
            switchBuilder = s => s.WhenNotMatched(Level.Debug, Color.Azure);

        maker.UseRealtime();
        var switches = maker.UseLocalSwitchSource();
        switchBuilder(switches);

        maker.Build();

        builder.Host.ConfigureLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddProvider(new LoggerProvider());
            lb.SetMinimumLevel(LogLevel.Trace);

        });


        using var logger = WatchFactoryLocator.Factory.GetLogger("Fabrica.Bootstrap");



        // *****************************************************************
        logger.Debug("Attempting to build BootstrapModule");
        var bootstrap = configuration.Get<TModule>();
        bootstrap.Configuration = configuration;


        await bootstrap.OnConfigured();



        // *****************************************************************
        logger.Debug("Attempting to configure services");
        builder.Host.ConfigureServices(s =>
        {

            s.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>();

            bootstrap.ConfigureServices(s);

            s.AddHostedService<TService>();

        });



        // *****************************************************************
        logger.Debug("Attempting to configure Autofac conatainer");
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


            bootstrap.ConfigureContainer(cb);


        }));



        // *****************************************************************
        logger.Debug("Attempting to configure Kestrel");
        builder.WebHost.UseKestrel(op =>
        {

            if (bootstrap.AllowAnyIp)
                op.ListenAnyIP(bootstrap.ListeningPort);
            else
                op.ListenLocalhost(bootstrap.ListeningPort);

        });



        // *****************************************************************
        logger.Debug("Attempting to configure web app");
        var app = builder.Build();
        bootstrap.ConfigureWebApp(app);

        var status = new
        {
            Environment = app.Environment.EnvironmentName,
            WebPootPath = app.Environment.WebRootPath,
            ContentPath = app.Environment.ContentRootPath,
            Urls = string.Join(',', app.Urls),
            bootstrap.ListeningPort,
            bootstrap.AllowAnyIp,
            bootstrap.MissionName,
            bootstrap.RunningAsMission
        };

        logger.LogObject(nameof(status), status);



        // *****************************************************************
        return app;


    }


}
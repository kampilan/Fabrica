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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
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


        var bootstrap = configuration.Get<TModule>();

        await bootstrap.OnConfigured();


        // *****************************************************************

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

                bootstrap.ConfigureContainer(cb);


            }))
            .ConfigureLogging(lb =>
            {
                lb.ClearProviders();
                lb.AddProvider(new LoggerProvider());
                lb.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices(s =>
            {

                s.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>();
                bootstrap.ConfigureServices(s);
                s.AddHostedService<TService>();

            });

        var app = builder.Build();
        bootstrap.ConfigureWebApp(app);

        return app;

    }


    public static WebApplicationBuilder BootstrapDebugAppliance<TModule>(this WebApplicationBuilder builder, string localConfigFile="", Action<SwitchSource> switchBuilder= null ) where TModule : Module
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


        // *****************************************************************
        builder.Host.ConfigureServices((context, collection) => collection.AddSingleton<IHostLifetime, ConsoleLifetime>());

        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(configuration)
                .As<IConfiguration>()
                .SingleInstance();

            var module = configuration.Get<TModule>();
            cb.RegisterModule(module);


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


}
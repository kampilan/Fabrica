using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Configuration.Yaml;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Process;
using Fabrica.Watch;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

    public static async Task<WebApplication> Bootstrap<TModule,TService>( string localConfigFile=null ) where TModule : BootstrapModule where TService: InitService
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger("Fabrica.OneAppliance.Bootstrap");

        try
        {


            // *****************************************************************
            logger.Debug("Loading Configuration");
            var cfgb = new ConfigurationBuilder();

            cfgb
                .AddYamlFile("configuration.yml", true)
                .AddJsonFile("environment.json", true)
                .AddJsonFile("mission.json", true);

            if (!string.IsNullOrWhiteSpace(localConfigFile))
                cfgb.AddYamlFile(localConfigFile, true);

            var configuration = cfgb.Build();



            // *****************************************************************
            logger.Debug("Building BootstrapModule");
            var bootstrap = configuration.Get<TModule>();
            bootstrap.Configuration = configuration;



            // *****************************************************************
            logger.Debug("Configuring Watch");
            bootstrap.ConfigureWatch();



            // *****************************************************************
            logger.Debug("Bootstrapping Appliance");
            var app = await bootstrap.Boot<TService>();



            // *****************************************************************
            return app;


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Bootstrap failed");
            throw;
        }

    }



}
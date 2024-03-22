// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToUsingDeclaration

using Fabrica.Configuration.Yaml;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One.Appliance;

public static class Appliance
{

    public static async Task<IAppliance> Bootstrap<TBootstrap>( string path="", string localConfigFile = null! ) where TBootstrap : IBootstrap
    {

        var app = await Bootstrap<TBootstrap, InitService>(path,localConfigFile);

        return app;

    }    

    public static async Task<IAppliance> Bootstrap<TBootstrap,TService>(string path = "", string localConfigFile=null!) where TBootstrap : IBootstrap where TService: class, IHostedService
    {

        IAppliance app;
        try
        {

            using var logger = WatchFactoryLocator.Factory.GetLogger("Fabrica.Appliance.Bootstrap");


            // *****************************************************************
            logger.Debug("Loading Configuration");
            var cfgb = new ConfigurationBuilder();

            cfgb
                .AddYamlFile("configuration.yml", true)
                .AddEnvironmentVariables()
                .AddJsonFile("environment.json", true)
                .AddJsonFile("mission.json", true);

            if (!string.IsNullOrWhiteSpace(localConfigFile))
                cfgb.AddYamlFile(localConfigFile, true);

            var configuration = cfgb.Build();



            // *****************************************************************
            logger.Debug("Building BaseBootstrap");
            var bootstrap = configuration.Get<TBootstrap>();
            if (bootstrap is null)
                throw new InvalidOperationException("Could not build Bootstrap from Configuration binding. Verify configuration files exist.");


            bootstrap.Configuration = configuration;



            // *****************************************************************
            logger.Debug("Configuring Watch");
            bootstrap.ConfigureWatch();



            // *****************************************************************
            logger.Debug("Bootstrapping Appliance");
            app = await bootstrap.Boot<TService>(path);


        }
        catch (Exception cause)
        {
            var el = WatchFactoryLocator.Factory.GetLogger("Fabrica.Appliance.Bootstrap");
            el.Error(cause, "Bootstrap failed");
            throw;
        }


        // *****************************************************************
        return app;


    }



}
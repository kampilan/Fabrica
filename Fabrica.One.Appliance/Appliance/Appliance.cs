// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToUsingDeclaration

using Fabrica.Configuration.Yaml;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;

namespace Fabrica.One.Appliance;

public static class Appliance
{


    public static async Task<IAppliance> Bootstrap<TBootstrap>(string path = "", string localConfigFile=null!) where TBootstrap : IBootstrap
    {
        
        IBootstrap bootstrap=null!;
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
            logger.Debug("BuildingBootstrap");
            bootstrap = configuration.Get<TBootstrap>() ?? throw new InvalidOperationException("Could not build Bootstrap from Configuration binding. Verify configuration files exist.");

            bootstrap.ApplicationBaseDirectory = string.IsNullOrWhiteSpace(path) ? AppDomain.CurrentDomain.BaseDirectory : path;
            bootstrap.Configuration = configuration;



            // *****************************************************************
            logger.Debug("Configuring Watch");
            bootstrap.ConfigureWatch();



            // *****************************************************************
            logger.Debug("Bootstrapping Appliance");
            app = await bootstrap.Boot();


        }
        catch (Exception cause)
        {
            var el = WatchFactoryLocator.Factory.GetLogger("Fabrica.One.Appliance.Bootstrap");
            el.ErrorWithContext(cause, bootstrap, "Bootstrap failed");
            throw;
        }


        // *****************************************************************
        return app;


    }



}
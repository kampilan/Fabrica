using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Microsoft.Extensions.Configuration;

namespace Fabrica.Monitor.Appliance;

public class TheBootstrap: KestrelBootstrap<TheModule,InitService>
{


    protected override void ConfigureApp(ConfigurationBuilder builder)
    {

        // *****************************************************************
        builder
            .AddYamlFile("configuration.yml", true)
            .AddJsonFile("environment.json", true)
            .AddJsonFile("mission.json", true);

    }


}
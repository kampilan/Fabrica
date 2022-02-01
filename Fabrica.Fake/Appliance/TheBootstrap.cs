using System.Drawing;
using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.Configuration;

namespace Fabrica.Fake.Appliance;

public class TheBootstrap: KestrelModuleBootstrap<TheModule,FakeInitService>
{



#if DEBUG

    protected override void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenMatched("Fabrica.Diagnostics.Http", "", Level.Debug, Color.Thistle)
            .WhenMatched("Fabrica.Fake", "", Level.Debug, Color.LightSalmon)
            .WhenMatched("Microsoft", "", Level.Warning, Color.BurlyWood)
            .WhenNotMatched(Level.Warning, Color.Azure);

        maker.Build();

    }


    protected override void ConfigureApp(ConfigurationBuilder builder)
    {

        // *****************************************************************
        builder
            .AddYamlFile("configuration.yml", true)
            .AddYamlFile("e:/local/fake/local.yml", true);

    }

#endif



}
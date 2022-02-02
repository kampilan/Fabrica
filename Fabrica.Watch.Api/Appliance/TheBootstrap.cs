using System.Drawing;
using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.Configuration;

namespace Fabrica.Watch.Api.Appliance;

public class TheBootstrap: KestrelBootstrap<TheModule,InitService>
{


#if DEBUG

    protected override void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenMatched("Fabrica.Diagnostics.Http", "", Level.Debug, Color.Thistle)
            .WhenNotMatched(Level.Debug, Color.Azure);

        maker.Build();

    }


    protected override void ConfigureApp(ConfigurationBuilder builder)
    {

        // *****************************************************************
        builder
            .AddYamlFile("configuration.yml", true)
            .AddYamlFile("e:/locals/watch/local.yml", true);

    }

#endif



}
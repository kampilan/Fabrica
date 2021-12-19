using System.Drawing;
using Fabrica.Api.Support.Conventions;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Fabrica.Models.Serialization;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Fabrica.Fake.Appliance;

public class TheBootstrap: KestrelBootstrap<TheModule,TheOptions,FakeInitService>
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


    protected override void ConfigureServices(IServiceCollection services)
    {

        services.AddHostedService<FakeInitService>();

        services.AddMvc(builder =>
            {


                if (Options.RequiresAuthentication)
                {
                    builder.Conventions.Add(new DefaultAuthorizeConvention<TokenAuthorizationFilter>());
                }

                builder.Filters.Add(typeof(ExceptionFilter));
                builder.Filters.Add(typeof(ResultFilter));

            })
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = new ModelContractResolver();
                opt.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            });


        services.Configure<ForwardedHeadersOptions>(options =>
        {

            options.RequireHeaderSymmetry = false;
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

        });


        if( Options.RequiresAuthentication )
        {
            services.AddProxyTokenAuthentication();
            services.AddProxyTokenAuthorization();
        }


    }


    protected override void ConfigureWebApp(IApplicationBuilder builder)
    {

        builder.UsePipelineMonitor();
        builder.UseDebugMode();

        builder.UseRequestLogging();

        builder.UseForwardedHeaders();

        builder.UseRouting();

        if (Options.RequiresAuthentication)
        {
            builder.UseAuthentication();
            builder.UseAuthorization();
        }



        builder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


    }

}
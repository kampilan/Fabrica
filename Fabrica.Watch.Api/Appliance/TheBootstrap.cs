using System.Collections.Generic;
using Autofac;
using Fabrica.Api.Support.Conventions;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Fabrica.Models.Serialization;
using Fabrica.Utilities.Drawing;
using Fabrica.Watch.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Fabrica.Watch.Api.Appliance
{


    public class TheBootstrap: KestrelBootstrap<TheModule,WatchOptions>
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
                .AddYamlFile("local.yml", true);

        }

#endif


        protected override void ConfigureServices(IServiceCollection services)
        {


            services.AddMvc(builder =>
            {
#if DEBUG

#else
                builder.Conventions.Add(new DefaultAuthorizeConvention<TokenAuthorizationFilter>());
#endif
                builder.Filters.Add(typeof(ExceptionFilter));
                builder.Filters.Add(typeof(ResultFilter));

            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new ModelContractResolver();
                    opt.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                });


            services.Configure<ForwardedHeadersOptions>(options =>
            {

                options.RequireHeaderSymmetry = false;
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

            });

#if DEBUG

#else
            services.AddProxyTokenAuthentication();
            services.AddProxyTokenAuthorization();
#endif



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("data", new OpenApiInfo { Title = "Fabrica.Watch", Version = "v1" });
                c.EnableAnnotations();
            });

            services.AddSwaggerGenNewtonsoftSupport();


        }


        protected override void ConfigureWebApp(IApplicationBuilder builder)
        {

            builder.UsePipelineMonitor();
            builder.UseDebugMode();

            builder.UseRequestLogging();

            builder.UseForwardedHeaders();


            builder.UseSwagger(o =>
            {
                o.PreSerializeFilters.Add((d, r) =>
                {
                    var url = $"https://{r.Host}";
                    d.Servers = new List<OpenApiServer> { new() { Url = url } };
                });


            });

            builder.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("data/swagger.json", "Fabrica.Watch");
            });


            builder.UseRouting();

#if DEBUG

#else
            builder.UseAuthentication();
            builder.UseAuthorization();
#endif

            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }


    }


}




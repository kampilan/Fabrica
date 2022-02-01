using System.Collections.Generic;
using Autofac;
using Fabrica.Api.Support.Conventions;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Models.Serialization;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Api.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Fabrica.Watch.Api.Appliance;

public class TheModule: BootstrapModule
{

    public string WatchEventStoreUri { get; set; } = "";


    public string TokenSigningKey { get; set; } = "";

    public override void ConfigureServices(IServiceCollection services)
    {

        services.AddMvc(builder =>
            {

                if (RequiresAuthentication)
                    builder.Conventions.Add(new DefaultAuthorizeConvention<TokenAuthorizationFilter>());

                builder.Filters.Add(typeof(ExceptionFilter));
                builder.Filters.Add(typeof(ResultFilter));

            })
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


        if( RequiresAuthentication )
        {
            services.AddProxyTokenAuthentication();
            services.AddProxyTokenAuthorization();
        }


        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("data", new OpenApiInfo { Title = "Fabrica.Watch", Version = "v1" });
            c.EnableAnnotations();
        });

        services.AddSwaggerGenNewtonsoftSupport();



    }


    public override void ConfigureContainer(ContainerBuilder builder)
    {

        builder.AddCorrelation();


        builder.AddProxyTokenEncoder(TokenSigningKey);

        builder.Register(c =>
            {

                var corr = c.Resolve<ICorrelation>();

                var comp = new WatchSinkCache(corr);
                comp.WatchEventStoreUri = WatchEventStoreUri;

                return comp;

            })
            .AsSelf()
            .SingleInstance()
            .AutoActivate();

    }


    public override void ConfigureWebApp(IApplicationBuilder builder)
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


        if( RequiresAuthentication )
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
using Autofac;
using Fabrica.Api.Support.Conventions;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Aws;
using Fabrica.Utilities.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Reflection;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Api.Support.Swagger;
using Fabrica.Models.Serialization;
using Fabrica.Rules;

namespace Fabrica.Repository.Appliance;

public class TheBootstrap: BaseBootstrap, IAwsCredentialModule, IRepositoryConfiguration
{


    public string Profile { get; set; } = "";
    public string RegionName { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public bool RunningOnEC2 { get; set; } = true;


    public string RepositoryContainer { get; set; } = "";
    public string PermanentRoot { get; set; } = "";
    public string TransientRoot { get; set; } = "";
    public string ResourceRoot { get; set; } = "";



    public override void ConfigureServices(IServiceCollection services)
    {

        using var logger = EnterMethod();


        services.AddMvcCore(builder =>
        {

            if( RequiresAuthentication )
                builder.Conventions.Add(new DefaultAuthorizeConvention<TokenAuthorizationFilter>());

            builder.Filters.Add(typeof(ExceptionFilter));
            builder.Filters.Add(typeof(ResultFilter));

        })
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver           = new ModelContractResolver();
                opt.SerializerSettings.DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate;
                opt.SerializerSettings.NullValueHandling          = NullValueHandling.Ignore;
                opt.SerializerSettings.DateTimeZoneHandling       = DateTimeZoneHandling.Utc;
                opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                opt.SerializerSettings.ReferenceLoopHandling      = ReferenceLoopHandling.Ignore;
            })
            .AddApplicationPart(GetType().Assembly)
            .AddApiExplorer()
            .AddAuthorization()
            .AddFormatterMappings()
            .AddDataAnnotations();


        services.Configure<ForwardedHeadersOptions>(options =>
        {

            options.RequireHeaderSymmetry = false;
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

        });


        if( RequiresAuthentication)    
            services.AddProxyTokenAuthentication();


        services.AddAuthorization(op =>
        {
            op.AddPolicy(TokenConstants.Policy, b => b.RequireAuthenticatedUser());
            op.AddPolicy("MustBeManager", b => b.RequireAuthenticatedUser().RequireRole("manager", "admin", "automation"));
            op.AddPolicy("MustBeAdmin", b => b.RequireAuthenticatedUser().RequireRole("admin","automation"));
            op.AddPolicy("MustBeAdminNoAutomation", b => b.RequireAuthenticatedUser().RequireRole("admin"));
            op.AddPolicy("MustBeAutomationOnly", b => b.RequireAuthenticatedUser().RequireRole("automation"));
        });


        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("data", new OpenApiInfo { Title = "Fabrica Repository API", Version = "v1" });
            c.EnableAnnotations();
            c.SchemaFilter<NoAdditionalPropertiesFilter>();

            c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    return new[] { controllerActionDescriptor.ControllerName };
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            c.OrderActionsBy((api) => $"{api.GroupName ?? ""}_{api.HttpMethod}");

            c.DocInclusionPredicate((_,_) => true);

        });

        services.AddSwaggerGenNewtonsoftSupport();


    }

    public override void ConfigureContainer(ContainerBuilder builder)
    {

        using var logger = EnterMethod();



        // *****************************************************
        builder.UseAws(this)
            .AddUrlProvider(this);


        // *****************************************************
        builder.AddProxyTokenEncoder(TokenSigningKey);


        // *****************************************************

        builder.UseRules();
        builder.AddRules(Assembly.GetExecutingAssembly(), typeof(TheBootstrap).Assembly);


    }

    public override void ConfigureWebApp(WebApplication app)
    {

        using var logger = EnterMethod();

        app.UsePipelineMonitor();
        app.UseDebugMode();

        app.UseRequestLogging();

        app.UseForwardedHeaders();

        app.UseSwagger(o =>
        {

            o.PreSerializeFilters.Add((d, r) =>
            {
                var url = $"https://{r.Host}";
                d.Servers = new List<OpenApiServer> { new() { Url = url } };
            });


        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("data/swagger.json", "Fabrica Repository API" );
        });


        app.UseRouting();


        if( RequiresAuthentication )
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


    }






}

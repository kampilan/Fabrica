using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Static.Appliance
{


    public class TheBootstrap: KestrelBootstrap<TheModule,ApplianceOptions>
    {


        protected override void ConfigureServices( IServiceCollection services )
        {

            services.AddMvc(o =>
                {
                    o.Filters.Add(typeof(ExceptionFilter));
                    o.Filters.Add(typeof(ResultFilter));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(ConfigureJsonForRto);

        }

        protected override void ConfigureWebApp( IApplicationBuilder builder )
        {

            var scope = builder.ApplicationServices.GetAutofacRoot();

            builder.UsePipelineMonitor();
            builder.UseDebugMode();
            builder.UseRequestLogging();

            var options = scope.Resolve<FileServerOptions>();
            builder.UseFileServer(options);

        }


    }


}

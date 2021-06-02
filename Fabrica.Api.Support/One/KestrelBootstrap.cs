using Autofac;
using Fabrica.Models.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.One
{


    public abstract class KestrelBootstrap<TModule,TOptions> : AbstractBootstrap<TModule,TOptions> where TModule : Module where TOptions: IApplianceOptions
    {

        protected abstract void ConfigureServices(IServiceCollection services);

        protected abstract void ConfigureWebApp(IApplicationBuilder builder);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder
                .UseKestrel(op =>
                {

                    if( Options.AllowAnyIp )
                        op.ListenAnyIP(Options.ListeningPort);
                    else
                        op.ListenLocalhost(Options.ListeningPort);

                })
                .ConfigureServices( ConfigureServices )
                .Configure( ConfigureWebApp );

        }

        protected virtual void ConfigureJsonForRto( MvcNewtonsoftJsonOptions opt )
        {

            opt.SerializerSettings.ContractResolver           = new ModelContractResolver();
            opt.SerializerSettings.DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate;
            opt.SerializerSettings.NullValueHandling          = NullValueHandling.Ignore;
            opt.SerializerSettings.DateTimeZoneHandling       = DateTimeZoneHandling.Utc;
            opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            opt.SerializerSettings.ReferenceLoopHandling      = ReferenceLoopHandling.Serialize;

        }

        protected virtual void ConfigureCors( CorsOptions opt )
        {

            opt.AddPolicy("AllowAll", b =>
            {
                b.AllowAnyOrigin();
                b.AllowAnyHeader();
                b.AllowAnyMethod();
                b.AllowCredentials();
            });

        }


    }


}

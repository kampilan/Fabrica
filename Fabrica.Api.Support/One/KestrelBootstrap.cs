using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Api.Support.One;

public abstract class KestrelBootstrap<TModule,TInit>: AbstractBootstrap<TModule> where TModule: BootstrapModule where TInit : InitService
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder
            .UseKestrel(op =>
            {

                if( Module.AllowAnyIp )
                    op.ListenAnyIP( Module.ListeningPort );
                else
                    op.ListenLocalhost( Module.ListeningPort );

            })
            .ConfigureServices(c =>
            {
                c.AddHostedService<TInit>();
                Module.ConfigureServices(c);
            })
            .Configure(b=> Module.ConfigureWebApp(b));

    }

}
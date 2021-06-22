using System.Net.Http;
using Autofac;
using Fabrica.Http;
using Fabrica.Utilities.Container;

namespace Fabrica.Identity
{

    public static class AutofacExtensions
    {


        public static ContainerBuilder UseOidcAccessTokenSource( this ContainerBuilder builder, IOidcConfiguration config )
        {

            builder.AddHttpClient("OidcEndpoint");

            builder.Register(c =>
                {

                    var corr    = c.Resolve<ICorrelation>();
                    var factory = c.Resolve<IHttpClientFactory>();

                    var comp = new OidcAccessTokenSource(config, factory, corr);

                    return comp;

                })
                .As<IAccessTokenSource>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();


            return builder;

        }


    }


}

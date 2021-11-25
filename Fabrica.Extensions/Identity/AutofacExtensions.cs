using System.Net.Http;
using Autofac;
using Fabrica.Http;
using Fabrica.Utilities.Container;

namespace Fabrica.Identity
{

    public static class AutofacExtensions
    {


        public static ContainerBuilder AddAccessTokenSource( this ContainerBuilder builder, string tokenSourceName, ICredentialGrant grant, string metaEndpoint="", string toeknEndpoint="" )
        {

            var clientName = $"TokenSource:{tokenSourceName}";

            builder.AddHttpClient( clientName );

            builder.Register(c =>
                {

                    var corr    = c.Resolve<ICorrelation>();
                    var factory = c.Resolve<IHttpClientFactory>();

                    var comp = new OidcAccessTokenSource(corr, factory, clientName, grant, metaEndpoint, toeknEndpoint );

                    return comp;

                })
                .As<IAccessTokenSource>()
                .Named<IAccessTokenSource>( tokenSourceName )
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();


            return builder;

        }


    }


}

using System;
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
                .As<IRequiresStart>()
                .SingleInstance()
                .AutoActivate();


            return builder;

        }

        public static ContainerBuilder AddClientCredentialAccessTokenSource<T>(this ContainerBuilder builder, string tokenSourceName, Func<T, string> audEx, Func<T,string> idEx, Func<T, string> secretEx, string metaEndpoint = "", string tokenEndpoint = "") where T: class
        {

            var clientName = $"TokenSource:{tokenSourceName}";

            builder.AddHttpClient(clientName);

            builder.Register(c =>
                {

                    
                    var corr    = c.Resolve<ICorrelation>();
                    var factory = c.Resolve<IHttpClientFactory>();
                    var secrets = c.Resolve<T>();

                    var grant = new ClientCredentialGrant
                    {
                        Audience     = audEx(secrets),
                        ClientId     = idEx(secrets),
                        ClientSecret = secretEx(secrets)
                    };

                    var comp = new OidcAccessTokenSource(corr, factory, clientName, grant, metaEndpoint, tokenEndpoint);

                    return comp;

                })
                .As<IAccessTokenSource>()
                .Named<IAccessTokenSource>(tokenSourceName)
                .As<IRequiresStart>()
                .SingleInstance()
                .AutoActivate();


            return builder;

        }



    }


}

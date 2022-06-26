using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using Fabrica.Http;
using Fabrica.Utilities.Container;

namespace Fabrica.Identity;

public static class AutofacExtensions
{


    public static ContainerBuilder AddClientCredentialGrant( this ContainerBuilder builder, string clientId, string clientSecret, IDictionary<string, string> additional = null )
    {

        var grant = new ClientCredentialGrant
        {
            ClientId     = clientId,
            ClientSecret = clientSecret
        };


        if( additional is {Count: > 0} )
        {
            foreach(var p in additional)
                grant.Body.Add(p.Key, p.Value);
        }


        builder.RegisterInstance(grant)
            .AsSelf()
            .As<ICredentialGrant>()
            .SingleInstance();

        return builder;

    }


    public static ContainerBuilder AddAccessTokenSource( this ContainerBuilder builder, string tokenSourceName, ICredentialGrant grant, string metaEndpoint="", string tokenEndpoint="" )
    {

        var clientName = $"TokenSource:{tokenSourceName}";

        builder.AddHttpClient( clientName );

        builder.Register(c =>
            {

                var corr    = c.Resolve<ICorrelation>();
                var factory = c.Resolve<IHttpClientFactory>();

                var comp = new OidcAccessTokenSource(corr, factory, clientName, grant, metaEndpoint, tokenEndpoint );

                return comp;

            })
            .As<IAccessTokenSource>()
            .Named<IAccessTokenSource>( tokenSourceName )
            .As<IRequiresStart>()
            .SingleInstance()
            .AutoActivate();


        return builder;

    }


    public static ContainerBuilder AddClientCredentialAccessTokenSource(this ContainerBuilder builder, Action<ClientCredentialGrant> grantBuilder, string tokenSourceName, string metaEndpoint = "", string tokenEndpoint = "")
    {

        var clientName = $"TokenSource:{tokenSourceName}";

        builder.AddHttpClient(clientName);

        builder.Register(c =>
            {

                var corr    = c.Resolve<ICorrelation>();
                var factory = c.Resolve<IHttpClientFactory>();

                var grant = new ClientCredentialGrant();
                grantBuilder(grant);

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


    public static ContainerBuilder AddClientCredentialAccessTokenSource<TSecrets>(this ContainerBuilder builder, Action<TSecrets, ClientCredentialGrant> grantBuilder, string tokenSourceName, string metaEndpoint = "", string tokenEndpoint = "") where TSecrets: class
    {

        var clientName = $"TokenSource:{tokenSourceName}";

        builder.AddHttpClient(clientName);

        builder.Register(c =>
            {
                    
                var corr    = c.Resolve<ICorrelation>();
                var factory = c.Resolve<IHttpClientFactory>();
                var secrets = c.Resolve<TSecrets>();

                var grant = new ClientCredentialGrant();

                grantBuilder(secrets, grant);

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

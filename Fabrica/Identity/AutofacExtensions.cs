// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using Fabrica.Utilities.Container;

namespace Fabrica.Identity;

public static class AutofacExtensions
{


    public static ContainerBuilder AddClientCredentialGrant( this ContainerBuilder builder, string name, string metaEndpoint, string clientId, string clientSecret, string tokenEndpoint="", IDictionary<string, string> additional = null )
    {

        var grant = new ClientCredentialGrant
        {
            MetaEndpoint  = metaEndpoint,
            ClientId      = clientId,
            ClientSecret  = clientSecret
        };


        if( !string.IsNullOrWhiteSpace(tokenEndpoint) )
            grant.TokenEndpoint = tokenEndpoint;

        if( additional is {Count: > 0} )
        {
            foreach (var p in additional)
                grant.Additional[p.Key] = p.Value;
        }


        builder.RegisterInstance(grant)
            .AsSelf()
            .As<ICredentialGrant>()
            .Named<ICredentialGrant>(name)
            .SingleInstance();

        return builder;

    }


    public static ContainerBuilder AddResourceOwnerGrant(this ContainerBuilder builder, string name, string metaEndpoint, string clientId, string clientSecret, string userName, string password, string tokenEndpoint = "", IDictionary<string, string> additional = null )
    {

        var grant = new ResourceOwnerGrant
        {
            MetaEndpoint = metaEndpoint,
            ClientId     = clientId,
            ClientSecret = clientSecret,
            UserName     = userName,
            Password     = password
        };


        if (!string.IsNullOrWhiteSpace(tokenEndpoint))
            grant.TokenEndpoint = tokenEndpoint;

        if (additional is { Count: > 0 })
        {
            foreach (var p in additional)
                grant.Additional[p.Key] = p.Value;
        }


        builder.RegisterInstance(grant)
            .AsSelf()
            .As<ICredentialGrant>()
            .Named<ICredentialGrant>(name)
            .SingleInstance();

        return builder;

    }


    public static ContainerBuilder AddAccessTokenSource( this ContainerBuilder builder, string tokenSourceName )
    {

        builder.Register(c =>
            {

                var corr    = c.Resolve<ICorrelation>();
                var factory = c.Resolve<IHttpClientFactory>();
                var grant   = c.ResolveNamed<ICredentialGrant>( tokenSourceName );

                var comp = new OidcAccessTokenSource( corr, factory, grant );

                return comp;

            })
            .As<IAccessTokenSource>()
            .Named<IAccessTokenSource>( tokenSourceName )
            .As<IRequiresStart>()
            .SingleInstance()
            .AutoActivate();


        return builder;

    }






}

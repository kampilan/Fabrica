using System.Collections.Generic;
using Autofac;
using Fabrica.Utilities.Container;

namespace Fabrica.Identity;

public static class AutofacExtensions
{


    public static ContainerBuilder UseAuth0IdentityProvider( this ContainerBuilder builder, string tokenSourceName, string domain )
    {


        builder.Register(c =>
            {

                var corr   = c.Resolve<ICorrelation>();
                var source = c.ResolveNamed<IAccessTokenSource>(tokenSourceName);

                var comp = new Auth0IdentityProvider(corr, source, domain);

                return comp;

            })
            .AsSelf()
            .As<IIdentityProvider>()
            .InstancePerLifetimeScope();


        return builder;

    }


    private static string Auth0Management => "";

    public static ContainerBuilder UseAuth0IdentityProvider(this ContainerBuilder builder, string tokenEndpoint, string clientId, string clientSecret, string audience, string domain )
    {

        var additional = new Dictionary<string, string>
        {
            ["audience"] = audience
        };

        builder.AddClientCredentialGrant(nameof(Auth0Management), "", clientId, clientSecret, tokenEndpoint, additional);
        builder.AddAccessTokenSource(nameof(Auth0Management));

        builder.Register(c =>
            {

                var corr = c.Resolve<ICorrelation>();
                var source = c.ResolveNamed<IAccessTokenSource>(nameof(Auth0Management));

                var comp = new Auth0IdentityProvider(corr, source, domain);

                return comp;

            })
            .AsSelf()
            .As<IIdentityProvider>()
            .InstancePerLifetimeScope();


        return builder;

    }



}
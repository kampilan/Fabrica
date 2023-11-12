using Autofac;
using Fabrica.Http;
using Fabrica.Utilities.Container;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Identity.Keycloak;

public static class AutofacExtensions
{


    public static ContainerBuilder AddKeycloakIdentityProvider(this ContainerBuilder builder )
    {

        builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();
                var factory = c.Resolve<IHttpClientFactory>();

                var comp = new KeycloakIdentityProvider( correlation, factory );

                return comp;

            })
            .AsSelf()
            .As<IIdentityProvider>()
            .InstancePerDependency();


        return builder;

    }


    public static ContainerBuilder AddKeycloakIdentityProvider(this ContainerBuilder builder, Action<TokenApiOptions> opts )
    {

        builder.AddTokenApiClient("Keycloak", opts);

        builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();
                var factory = c.Resolve<IHttpClientFactory>();

                var comp = new KeycloakIdentityProvider( correlation, factory );

                return comp;

            })
            .AsSelf()
            .As<IIdentityProvider>()
            .InstancePerDependency();


        return builder;

    }




}
using Autofac;
using Fabrica.Utilities.Container;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Identity;

public static class AutofacExtensions
{


    public static ContainerBuilder AddKeycloakIdentityProvider(this ContainerBuilder builder, string endpoint, string realm, string clientId, string clientSecret )
    {

        builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();

                var comp = new KeycloakIdentityProvider(correlation, endpoint, realm, clientId, clientSecret);

                return comp;

            })
            .AsSelf()
            .As<IIdentityProvider>()
            .InstancePerDependency();


        return builder;

    }

}
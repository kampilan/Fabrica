using Autofac;
using Fabrica.Utilities.Container;

namespace Fabrica.Identity
{

    
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

    }


}

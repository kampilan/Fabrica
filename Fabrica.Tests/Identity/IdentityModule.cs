using Autofac;
using Fabrica.Identity;
using Fabrica.Utilities.Container;

namespace Fabrica.Tests.Identity
{
    

    public class IdentityModule: Module, IOidcConfiguration
    {

        public string OidcMetaEndpoint { get; set; } = "https://auth-qa.contakt.world/auth/realms/contact-world-realm/.well-known/openid-configuration";
        public string OidcClientId { get; set; } = "meshtek-backend";
        public string OidcClientSecret { get; set; } = "99ce1821-6e16-4d99-94ed-5bebdfcd57e6";
        public string OidcAudience { get; set; } = "";


        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            builder.UseOidcAccessTokenSource(this);

        }


    }


}

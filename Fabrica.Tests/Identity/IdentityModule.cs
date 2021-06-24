using Autofac;
using Fabrica.Identity;
using Fabrica.Utilities.Container;

namespace Fabrica.Tests.Identity
{
    

    public class IdentityModule: Module
    {

        public string Auth0Domain { get; set; } = "";

        public string MetaEndpoint { get; set; } = "";
        public string TokenEndpoint { get; set; } = "";

        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string Audience { get; set; } = "";


        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            builder.AddAccessTokenSource( "Auth0Management", new ClientCredentialGrant { ClientId = ClientId, ClientSecret = ClientSecret, Audience = Audience }, MetaEndpoint, TokenEndpoint );

            builder.UseAuth0IdentityProvider("Auth0Management", Auth0Domain);

        }


    }


}

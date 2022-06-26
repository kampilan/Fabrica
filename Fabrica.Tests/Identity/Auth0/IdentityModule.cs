using Autofac;
using Fabrica.Identity;
using Fabrica.Utilities.Container;

namespace Fabrica.Tests.Identity.Auth0
{


    public class IdentityModule : Module
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

            var grant = new ClientCredentialGrant
            {
                MetaEndpoint = MetaEndpoint,
                ClientId = ClientId,
                ClientSecret = ClientSecret
            };

            if (!string.IsNullOrWhiteSpace(TokenEndpoint))
                grant.TokenEndpoint = TokenEndpoint;

            if (!string.IsNullOrWhiteSpace(Audience))
                grant.Body["audience"] = Audience;

            builder.AddAccessTokenSource("Auth0Management");

            builder.UseAuth0IdentityProvider("Auth0Management", Auth0Domain);

        }


    }


}

using System.Collections.Generic;
using Autofac;
using Fabrica.Identity;
using Fabrica.Utilities.Container;

namespace Fabrica.Tests.Identity.Auth0
{


    public class IdentityModule : Module
    {

        private static string Auth0Management => "";


        public string Auth0Domain { get; set; } = "";

        public string MetaEndpoint { get; set; } = "";
        public string TokenEndpoint { get; set; } = "";

        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string Audience { get; set; } = "";


        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            var additional = new Dictionary<string, string>
            {
                ["audience"] = Audience
            };


            builder.AddClientCredentialGrant(nameof(Auth0Management), "", ClientId, ClientSecret, TokenEndpoint, additional );

            builder.AddAccessTokenSource(nameof(Auth0Management));

            builder.UseAuth0IdentityProvider(nameof(Auth0Management), Auth0Domain);

        }


    }


}

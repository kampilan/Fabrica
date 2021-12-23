using Fabrica.Api.Support.One;

namespace Fabrica.Proxy.Appliance
{


    public class ProxyOptions: ApplianceOptions
    {


        public bool RunningOnEC2 { get; set; }
        public string ApplicationDiscriminator { get; set; } = "";
        public string DataProtectionParameterName { get; set; } = "";


        public bool ConfigureForAuthentication => !string.IsNullOrWhiteSpace(MetadataAddress);

        public bool IncludeUserAuthentication { get; set; } = true;
        public bool IncludeApiAuthentication { get; set; } = true;


        public string MetadataAddress { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";

        public string Audience { get; set; }

        public string Scopes { get; set; } = "";


        public string ProviderSignOutUri { get; set; } = "";


        public string LoginRoute { get; set; } = "/login";
        public string LogoutRoute { get; set; } = "/logout";
        public string PostLoginRedirectUri { get; set; } = "/";
        public string PostLogoutRedirectUri { get; set; } = "/";


    }
}
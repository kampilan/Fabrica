using Fabrica.Api.Support.One;
using Newtonsoft.Json;

namespace Fabrica.Proxy.Appliance
{


    [JsonObject(MemberSerialization.OptIn)]
    public class ProxyOptions: ApplianceOptions
    {

        public bool RunningOnEc2 { get; set; } = true;
        public string RegionName { get; set; } = "";
        public string ProfileName { get; set; } = "";


        public string ApplicationDiscriminator { get; set; } = "";
        public string DataProtectionParameterName { get; set; } = "";

        public string AwsSecretsId { get; set; } = "";

        
        [JsonProperty("oidc-client-id")]
        public string OidcClientId { get; set; } = "";

        [JsonProperty("oidc-client-secret")]
        public string OidcClientSecret { get; set; } = "";


        public bool UseSession { get; set; } = false;
        public string RedisConnectionStr { get; set; } = "";

        public bool ConfigureForAuthentication => !string.IsNullOrWhiteSpace(MetadataAddress);

        public bool IncludeUserAuthentication { get; set; } = true;
        public bool IncludeApiAuthentication { get; set; } = true;


        public string MetadataAddress { get; set; } = "";

        public string Audience { get; set; }

        public string Scopes { get; set; } = "";


        public string ProviderSignOutUri { get; set; } = "";


        public string LoginRoute { get; set; } = "/login";
        public string LogoutRoute { get; set; } = "/logout";
        public string PostLoginRedirectUri { get; set; } = "/";
        public string PostLogoutRedirectUri { get; set; } = "/";


    }
}
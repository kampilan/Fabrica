using System.Text.Json.Serialization;

namespace Fabrica.Identity.Keycloak.Models;

public class SocialLink
{

    [JsonPropertyName("socialProvider")]
    public string? SocialProvider { get; set; }

    [JsonPropertyName("socialUserId")]
    public string? SocialUserId { get; set; }

    [JsonPropertyName("socialUserName")]
    public string? SocialUserName { get; set; }


}
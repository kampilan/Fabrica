using System.Text.Json.Serialization;

namespace Fabrica.Identity.Keycloak.Models;

public class FederatedIdentity
{

    [JsonPropertyName("identityProvider")]
    public string? IdentityProvider { get; set; }
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

}
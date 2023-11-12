using System.Text.Json.Serialization;

namespace Fabrica.Identity.Keycloak.Models;

public class UserProfileMetadata
{

    [JsonPropertyName("attributes")]
    public ICollection<UserProfileAttributeMetadata>? Attributes { get; set; }

    [JsonPropertyName("groups")]
    public ICollection<UserProfileAttributeGroupMetadata>? Groups { get; set; }


}
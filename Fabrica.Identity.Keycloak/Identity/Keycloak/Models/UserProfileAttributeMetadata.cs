using System.Text.Json.Serialization;

namespace Fabrica.Identity.Keycloak.Models;

public class UserProfileAttributeMetadata
{

    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("required")]
    public bool? Required { get; set; }
    [JsonPropertyName("readonly")]
    public bool? Readonly { get; set; }
    [JsonPropertyName("annotations")]
    public IDictionary<string,object>? Annotations { get; set; }
    [JsonPropertyName("validators")]
    public IDictionary<string, IDictionary<string,object>>? Validators { get; set; }
    [JsonPropertyName("group")]
    public string? Group { get; set; }


}
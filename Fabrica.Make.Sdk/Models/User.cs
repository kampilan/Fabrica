using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk.Models;

public class User
{

    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

}
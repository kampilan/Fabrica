using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk.Models;

public class HookData
{

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("udt")]
    public int? Udt { get; set; }

    [JsonPropertyName("headers")]
    public bool? Headers { get; set; }

    [JsonPropertyName("method")]
    public bool? Method { get; set; }

    [JsonPropertyName("stringify")]
    public bool? Stringify { get; set; }

    [JsonPropertyName("teamId")]
    public int? TeamId { get; set; }


}
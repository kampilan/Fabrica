using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk.Models;

public class Schedule
{

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("interval")]
    public int? Interval { get; set; }

}
using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk.Models;

public class Pager
{

    [JsonPropertyName("sortBy")]
    public string SortBy { get; set; } = string.Empty;
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("sortDir")]
    public string SortDir { get; set; } = string.Empty;

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }

    [JsonPropertyName("returnTotalCount")]
    public bool? ReturnTotalCount { get; set; }

}
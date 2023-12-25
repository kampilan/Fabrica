using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk.Models;

public class Flag
{

    [JsonPropertyName("form")]
    public bool? Form { get; set; }

}
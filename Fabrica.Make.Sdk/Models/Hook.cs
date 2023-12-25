using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk.Models;

public class HookResponse
{

    [JsonPropertyName("hooks")]
    public IEnumerable<Hook> Hooks { get; set; } = new List<Hook>();

    [JsonPropertyName("pg")]
    public Pager Pg { get; set; } = new();

}


public class Hook
{

    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("teamId")]
    public int TeamId { get; set; }

    [JsonPropertyName("organizationId")]
    public int OrganizationId { get; set; }

    [JsonPropertyName("udid")]
    public string Udid { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("theme")]
    public string Theme { get; set; } = string.Empty;

    [JsonPropertyName("editable")]
    public bool? Editable { get; set; }

    [JsonPropertyName("flags")]
    public Flag? Flags { get; set; } = new();

    [JsonPropertyName("queueCount")]
    public int? QueueCount { get; set; }

    [JsonPropertyName("queueLimit")]
    public int? QueueLimit { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("gone")]
    public bool? Gone { get; set; }

    [JsonPropertyName("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("typeAppName")]
    public string TypeAppName { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public HookData? Data { get; set; } = new();

    [JsonPropertyName("scenarioId")]
    public int? ScenarioId { get; set; }

    [JsonPropertyName("scenarioName")]
    public string ScenarioName { get; set; } = string.Empty;

    [JsonPropertyName("scenarioIsActive")]
    public bool? ScenarioIsActive { get; set; }

    [JsonPropertyName("relay")]
    public string Relay { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;


}
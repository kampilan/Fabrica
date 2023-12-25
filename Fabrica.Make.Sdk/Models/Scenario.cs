using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk.Models;

public class ScenarioResponse
{

    [JsonPropertyName("scenarios")]
    public IEnumerable<Scenario> Scenarios { get; set; } = new List<Scenario>();

    [JsonPropertyName("pg")]
    public Pager Pg { get; set; } = new();

}


public class Scenario
{
    

    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("teamId")]
    public int TeamId { get; set; }

    [JsonPropertyName("hookId")]
    public int? HookId { get; set; }

    [JsonPropertyName("deviceId")]
    public int? DeviceId { get; set; }

    [JsonPropertyName("deviceScope")]
    public string DeviceScope { get; set; } = string.Empty;

    [JsonPropertyName("concept")]
    public bool? Concept { get; set; }


    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("folderId")]
    public int? FolderId { get; set; }

    [JsonPropertyName("slots")]
    public string Slots { get; set; } = string.Empty;


    [JsonPropertyName("isinvalid")]
    public bool? IsInvalid { get; set; }

    [JsonPropertyName("islinked")]
    public bool? IsLinked { get; set; }

    [JsonPropertyName("islocked")]
    public bool? IsLocked { get; set; }

    [JsonPropertyName("isPaused")]
    public bool? IsPaused { get; set; }

    [JsonPropertyName("usedPackages")]
    public ICollection<string>? Packages { get; set; } = new List<string>();


    [JsonPropertyName("lastEdit")]
    public DateTime? LastEdit { get; set; }

    [JsonPropertyName("scheduling")]
    public Schedule Scheduling { get; set; } = new ();

    [JsonPropertyName("isWaiting")]
    public bool? IsWaiting { get; set; }

    [JsonPropertyName("dlgCount")]
    public int DlqCount { get; set; }

    [JsonPropertyName("createdByUser")]
    public User? CreatedByUser { get; set; } = new();

    [JsonPropertyName("updatedByUser")]
    public User UpdatedByUser { get; set; } = new();

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }


    [JsonPropertyName("nextExec")]
    public DateTime? NextExec { get; set; }


}
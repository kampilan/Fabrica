// ReSharper disable CollectionNeverUpdated.Local

using System.Text.Json;
using System.Text.Json.Serialization;
using Fabrica.Models.Support;

namespace Fabrica.Rql;

public class BaseCriteria: ICriteria, IApiModel
{

    public string[]? Rql { get; set; }

    [JsonExtensionData]
    private Dictionary<string, JsonElement> Overposts { get; } = new();

    public bool IsOverposted() => Overposts.Count > 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;


}
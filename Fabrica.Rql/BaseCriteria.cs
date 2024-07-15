// ReSharper disable CollectionNeverUpdated.Local

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabrica.Rql;

public class BaseCriteria: ICriteria
{

    public string[]? Rql { get; set; }

    [JsonExtensionData]
    private Dictionary<string, JsonElement> Overposts { get; } = new();

    public bool IsOverposted() => Overposts.Count > 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;


}
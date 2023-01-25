// ReSharper disable CollectionNeverUpdated.Local

using Fabrica.Models.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Rql;

public class BaseCriteria: ICriteria, IApiModel
{

    public string[]? Rql { get; set; }

    [JsonExtensionData]
    private Dictionary<string, JToken> Overposts { get; } = new();

    public bool IsOverposted() => Overposts.Count > 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;


}
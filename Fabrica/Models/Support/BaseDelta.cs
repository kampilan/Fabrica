
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabrica.Models.Support;

public class BaseDelta: IApiModel
{

    [JsonExtensionData]
    private Dictionary<string,JsonElement> Overposts { get; } = new ();

    public bool IsOverposted() => Overposts.Count > 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;


}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Models.Support;

public class BaseDelta: IApiModel
{

    [JsonExtensionData]
    private Dictionary<string,JToken> Overposts { get; } = new ();

    public bool IsOverposted() => Overposts.Count > 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;


}
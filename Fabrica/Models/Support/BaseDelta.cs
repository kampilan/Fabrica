using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Models.Support
{


    public class BaseDelta
    {


        [JsonExtensionData]
        private Dictionary<string,JToken> Overposts { get; } = new ();

        public bool IsOverposted() => Overposts.Count > 0;

        public IEnumerable<string> GetOverpostNames() => Overposts.Keys;

        public DeltaPropertySet GetPropertySet() => new (this);


    }


}

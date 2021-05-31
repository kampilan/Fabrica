using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fabrica.Models.Patch.Builder
{


    public class ModelPatch
    {

        [DefaultValue(PatchVerb.Update)]
        [JsonConverter(typeof(StringEnumConverter))]
        public PatchVerb Verb { get; set; } = PatchVerb.Update;
        public string Model { get; set; } = "";
        public string Uid { get; set; } = "";


        [JsonIgnore]
        public bool IsMember => Membership != null;

        [DefaultValue(null)]
        public PropertyPath Membership { get; set; }
        public bool ShouldSerializeMembership() => Membership != null;


        public IDictionary<string,object> Properties { get; set; } = new Dictionary<string,object>();
        public bool ShouldSerializeProperties() => Properties.Count > 0;



    }

}

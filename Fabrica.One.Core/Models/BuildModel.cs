
using System;
using System.Text.Json.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;

namespace Fabrica.One.Models
{

    public class BuildModel: BaseReferenceModel
    {


        [JsonIgnore]
        public long Id { get; protected set; }

        [JsonIgnore] 
        public override string Uid { get; set; } = Base62Converter.NewGuid();


        public string Name { get; set; } = "";

        public string BuildNum { get; set; } = "";

        public DateTime BuildDate { get; set; } = DateTime.MinValue;

        public int BuildSize { get; set; }

        public string Checksum { get; set; } = "";

        public string Assembly { get; set; } = "";

        public int UseCount { get; set;} = 0;


        [JsonIgnore]
        public bool IsAbsoluteBuild
        {
            get
            {
                var x  = int.TryParse(BuildNum, out _ );
                return x;
            }
        }

        public override string ToString()
        {
            return $"{Name}-{BuildNum}";
        }

    }

}

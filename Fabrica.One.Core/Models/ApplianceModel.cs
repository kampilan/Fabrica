using System;
using System.Text.Json.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;

namespace Fabrica.One.Models
{
    
    public class ApplianceModel: BaseReferenceModel
    {

        [JsonIgnore]
        public override long Id { get; protected set; }

        [JsonIgnore]
        public override string Uid { get; set; } = Base62Converter.NewGuid();

        public string Name { get; set; } = "";

        public DateTime LatestBuildDate { get; set; } = DateTime.MinValue;

        public int TotalBuilds { get; set; } = 0;


    }

}

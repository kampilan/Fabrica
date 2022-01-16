using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fabrica.Utilities.Text;
using Json.Schema.Generation;

namespace Fabrica.One.Plan
{

    public class DeploymentUnit
    {


        [JsonIgnore] 
        public string Uid { get; set; } = Base62Converter.NewGuid();

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Alias { get; set; } = "";

        [Required]
        public string Build { get; set; } = "";

        [Required]
        public string Checksum { get; set; } = "";

        [Required]
        public string Assembly { get; set; } = "";

        [Required]
        public bool Deploy { get; set; }

        [Required]
        public bool WaitForStart { get; set; }

        [Required]
        public bool ShowWindow { get; set; }

        [Required]
        public JsonObject EnvironmentConfiguration { get; set; } = new JsonObject();


        [JsonIgnore]
        public Dictionary<string, object> MissionConfiguration { get; } = new Dictionary<string, object>();


        [JsonIgnore]
        public string RepositoryLocation { get; set; } = "";


        [JsonIgnore]
        public MemoryStream RepositoryContent { get; } = new MemoryStream();


        [JsonIgnore]
        public string InstallationLocation { get; set; } = "";

        [JsonIgnore]
        public string EnvironmentConfigLocation { get; set; } = "";

        [JsonIgnore]
        public string MissionConfigLocation { get; set; } = "";

        [JsonIgnore]
        public string ExecutionCommand { get; set; } = "";

        [JsonIgnore]
        public string ExecutionArguments { get; set; } = "";


        [JsonIgnore]
        public bool HasLoaded { get; set; }


        [JsonIgnore]
        public bool HasInstalled { get; set; }


    }


}

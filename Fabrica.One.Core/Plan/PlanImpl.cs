using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.Schema.Generation;

namespace Fabrica.One.Plan
{


    public class PlanImpl : IPlan
    {

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Environment { get; set; } = "";

        [Required]
        [MinLength(0)]
        public string RepositoryVersion { get; set; } = "";

        [Required]
        public bool DeployAppliances { get; set; }

        [Required]
        public bool StartAppliances { get; set; }

        [Required]
        public bool StartInParallel { get; set; }

        [Required]
        public bool AllAppliancesMustDeploy { get; set; }

        [Required]
        [Minimum(10)]
        [Maximum(30)]
        public int WaitForDeploySeconds { get; set; }

        [Required]
        [Minimum(10)]
        [Maximum(240)]
        public int WaitForStartSeconds { get; set; }

        [Required]
        [Minimum(10)]
        [Maximum(240)]
        public int WaitForStopSeconds { get; set; }


        [Required]
        [MinItems(1)]
        public List<DeploymentUnit> Deployments { get; set; } = new List<DeploymentUnit>();


        [JsonIgnore]
        public string RepositoryRoot { get; set; } = "";

        [JsonIgnore]
        public string InstallationRoot { get; set; } = "";


        public void SetRepositoryVersion( string version )
        {
            RepositoryVersion = version;
        }

    }


}

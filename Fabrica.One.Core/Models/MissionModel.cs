using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;

namespace Fabrica.One.Models
{


    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class MissionModel: BaseMutableModel<MissionModel>, INotifyPropertyChanged
    {

        public MissionModel()
        {
            Deployments = new List<DeploymentModel>();
        }

        [JsonIgnore]
        [Editable(false)]
        public override long Id { get; protected set; }

        [JsonIgnore]
        [Editable(false)]
        public override string Uid { get; set; } = Base62Converter.NewGuid();

        [JsonIgnore]
        [Editable(false)]
        public string RepositoryLocation { get; set; } = "";


        private string _name="";
        [Required]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        private string _environment = "";
        [Required]
        public string Environment
        {
            get => _environment;
            set => _environment = value;
        }

        [JsonIgnore]
        public string Fqmn => $"{Name}-{Environment}";

        private string _repositoryVersion = "";
        [Editable(false)]
        public string RepositoryVersion
        {
            get => _repositoryVersion;
            set => _repositoryVersion = value;
        }

        private bool _deployAppliances;
        [Required]
        public bool DeployAppliances
        {
            get => _deployAppliances;
            set => _deployAppliances = value;
        }
        private bool _startAppliances;
        [Required]
        public bool StartAppliances
        {
            get => _startAppliances;
            set => _startAppliances = value;
        }

        private bool _startInParallel;
        [Required]
        public bool StartInParallel
        {
            get => _startInParallel;
            set => _startInParallel = value;
        }

        private bool _allAppliancesMustDeploy;
        [Required]
        public bool AllAppliancesMustDeploy
        {
            get => _allAppliancesMustDeploy;
            set => _allAppliancesMustDeploy = value;
        }

        private int _waitForDeploySeconds;
        [Required]
        public int WaitForDeploySeconds
        {
            get => _waitForDeploySeconds;
            set => _waitForDeploySeconds = value;
        }

        private int _waitForStartSeconds;
        [Required]
        public int WaitForStartSeconds
        {
            get => _waitForStartSeconds;
            set => _waitForStartSeconds = value;
        }

        private int _waitForStopSeconds;
        [Required]
        public int WaitForStopSeconds
        {
            get => _waitForStopSeconds;
            set => _waitForStopSeconds = value;
        }


        private AggregateObservable<DeploymentModel> _deployments;
        [Editable(false)]
        public ICollection<DeploymentModel> Deployments
        {
            get => _deployments;
            set => _deployments = new AggregateObservable<DeploymentModel>(this, "Deployments", value);
        }


        public DeploymentModel AddDeployment( BuildModel build )
        {

            var model = new DeploymentModel
            {
                Name     = build.Name,
                Alias    = $"{build.Name}-{build.BuildNum}",
                Build    = build.BuildNum,
                Checksum = build.Checksum,
                Assembly = build.Assembly
            };

            Deployments.Add(model);

            return model;

        }

        public DeploymentModel AddDeployment()
        {

            var model = new DeploymentModel();

            Deployments.Add(model);

            return model;

        }


        private string _appConfigApplicationId = "";
        public string AppConfigApplicationId
        {
            get => _appConfigApplicationId;
            set => _appConfigApplicationId = value;
        }


        private string _appConfigConfigProfileId = "";
        public string AppConfigConfigProfileId
        {
            get => _appConfigConfigProfileId;
            set => _appConfigConfigProfileId = value;
        }


        private string _appConfigDeploymentStrategyId = "";
        public string AppConfigDeploymentStrategyId
        {
            get => _appConfigDeploymentStrategyId;
            set => _appConfigDeploymentStrategyId = value;
        }


        private string _appConfigEnvironmentId = "";
        public string AppConfigEnvironmentId
        {
            get => _appConfigEnvironmentId;
            set => _appConfigEnvironmentId = value;
        }


        public override string ToString()
        {
            return $"{Name} with {Deployments?.Count??0} Deployment(s)";
        }
    }

}

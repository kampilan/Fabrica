using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Fabrica.Utilities.Types;

namespace Fabrica.One.Plan
{


    [TypeConverter(typeof(PropertySorterConverter))]
    public class DeploymentUnit
    {


        [Category("1 - Identity")]
        [DisplayName("Name")]
        [Description("The name of the appliance")]
        [PropertySortOrder(1)]
        public string Name { get; set; } = "";

        [Category("1 - Identity")]
        [DisplayName("Alias")]
        [Description("The alias of the appliance. Useful when 2 instance of the same appliance are being deployed.")]
        [PropertySortOrder(2)]
        public string Alias { get; set; } = "";

        [Category("1 - Identity")]
        [DisplayName("Build")]
        [Description("The build number of the appliance.")]
        [PropertySortOrder(3)]
        public string Build { get; set; } = "";

        [Category("1 - Identity")]
        [DisplayName("Checksum")]
        [Description("The hash value of build.")]
        [PropertySortOrder(4)]
        public string Checksum { get; set; } = "";


        [Category("2 - Roots")]
        [DisplayName("Repository")]
        [Description("The path to the root of the appliance repository")]
        [PropertySortOrder(5)]
        public string RepositoryRoot { get; set; } = "";

        [Category("2 - Roots")]
        [DisplayName("Installation")]
        [Description("The path to the root of the installation directory")]
        [PropertySortOrder(6)]
        public string InstallationRoot { get; set; } = "";


        [Category("3 - Execution")]
        [DisplayName("Uid")]
        [Description("The unique id for this appliance deployment.")]
        [PropertySortOrder(7)]
        public string Id { get; set; } = "";

        [Category("3 - Execution")]
        [DisplayName("Event Prefix")]
        [Description("The Wait Event prefix used for interprocess communication between the host and the appliance.")]
        [PropertySortOrder(8)]
        public string EventNamePrefix { get; set; } = "";


        [Category("3 - Execution")]
        [DisplayName("Assembly")]
        [Description("The the name of the start assembly for this appliance")]
        [PropertySortOrder(9)]
        public string Assembly { get; set; } = "";

        [Category("3 - Execution")]
        [DisplayName("Type")]
        [Description("The type of execution used to start this appliance")]
        [PropertySortOrder(10)]
        public string ExecutionType { get; set; } = "";

        [Category("3 - Execution")]
        [DisplayName("Command")]
        [Description("The command used to start this appliance")]
        [PropertySortOrder(11)]
        public string ExecutionCommand { get; set; } = "";

        [Category("3 - Execution")]
        [DisplayName("Path")]
        [Description("The start path for this appliance")]
        [PropertySortOrder(12)]
        public string ExecutionPath { get; set; } = "";

        [Category("3 - Execution")]
        [DisplayName("Arguments")]
        [Description("The command line arguments passed at start for this appliance")]
        [PropertySortOrder(13)]
        public string ExecutionArguments { get; set; } = "";


        [Category("4 - Behavior")]
        [DisplayName("Deploy")]
        [Description("Should the appliance be deployed from the repository?")]
        [PropertySortOrder(14)]
        public bool Deploy { get; set; } = true;


        [Browsable(false)]
        public bool HasLoaded { get; set; } = false;
        [Browsable(false)]
        public bool HasInstalled { get; set; } = false;
        [Browsable(false)]
        public bool HasDeployed => HasLoaded && HasInstalled;

        [Category("4 - Behavior")]
        [DisplayName("Wait For Start")]
        [Description("Should this appliance be completely started before other appliances are started?")]
        [PropertySortOrder(15)]
        public bool WaitForStart { get; set; } = false;

        [Category("4 - Behavior")]
        [DisplayName("Show Window")]
        [Description("Should this appliance show its console window when started?")]
        [PropertySortOrder(16)]
        public bool ShowWindow { get; set; } = false;


        [Category("5 - Partial")]
        [DisplayName("Name")]
        [Description("Name of the partial appliance")]
        [PropertySortOrder(17)]
        public string PartialName { get; set; } = "";

        [Category("5 - Partial")]
        [DisplayName("Build")]
        [Description("The build number of the partial appliance")]
        [PropertySortOrder(18)]
        public string PartialBuild { get; set; } = "";

        [Category("5 - Partial")]
        [DisplayName("Checksum")]
        [Description("The hash value of build.")]
        [PropertySortOrder(19)]
        public string PartialChecksum { get; set; } = "";

        [Category("5 - Partial")]
        [DisplayName("Assembly")]
        [Description("The name of the assembly for this partial appliance")]
        [PropertySortOrder(20)]
        public string PartialAssembly { get; set; } = "";


        [Category("6 - Watch")]
        [DisplayName("Realtime")]
        [Description("Should realtime logging be used?")]
        [PropertySortOrder(21)]
        public bool RealtimeLogging { get; set; } = false;

        [Category("6 - Watch")]
        [DisplayName("Event Store URI")]
        [Description("The mongodb server uri where log events will be stored.")]
        [PropertySortOrder(22)]
        public string WatchEventStoreUri { get; set; } = "";

        [Category("6 - Watch")]
        [DisplayName("Domain Name")]
        [Description("The Watch domain that controls the logging for this appliance")]
        [PropertySortOrder(23)]
        public string WatchDomainName { get; set; } = "";


        [Category("7 - Environment")]
        [DisplayName("Name")]
        [Description("The name of the ebviroment that this appliance will run under.")]
        [PropertySortOrder(24)]
        public string Environment { get; set; } = "";


        [Category("8 - API")]
        [DisplayName("Listening Port")]
        [Description("What port should Kestrel listen on")]
        [PropertySortOrder(25)]
        public int ListeningPort { get; set; }

        [Category("8 - API")]
        [DisplayName("Requires Authentication")]
        [Description("This API appliance requires authentication")]
        [PropertySortOrder(26)]
        public bool RequiresAuthentication { get; set; } = true;


        [Category("9 - Location")]
        [DisplayName("Deployment")]
        [Description("The location of the appliance deloyment in the repository")]
        [PropertySortOrder(27)]
        public string DeploymentLocation { get; set; }
        [Browsable(false)]
        public MemoryStream DeploymentContent { get; } = new MemoryStream();

        [Category("9 - Location")]
        [DisplayName("Partial Deployment")]
        [Description("The location of the partial appliance deloyment in the repository")]
        [PropertySortOrder(28)]
        public string  PartialDeploymentLocation { get; set; }
        [Browsable(false)]
        public MemoryStream PartialDeploymentContent  { get; } = new MemoryStream();

        [Category("9 - Location")]
        [DisplayName("Configuration Type")]
        [Description("The file type of the configuration files")]
        [PropertySortOrder(29)]
        public string ConfigurationType { get; set; } = "";

        [Category("9 - Location")]
        [DisplayName("Configuration")]
        [Description("The location of the configuration file in the repository")]
        [PropertySortOrder(30)]
        public string ConfigurationLocation { get; set; } = "";
        [Browsable(false)]
        public MemoryStream ConfigurationContent { get; } = new MemoryStream();

        [Browsable(false)]
        public Dictionary<string,object> Configuration { get; set; } = new Dictionary<string, object>();


    }


}

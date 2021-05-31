using System.ComponentModel;
using Fabrica.Utilities.Types;

namespace Fabrica.One
{


    [TypeConverter(typeof(PropertySorterConverter))]
    public class ApplianceInfo
    {


        [Category("1 - Identity")]
        [DisplayName("Id")]
        [PropertySortOrder(1)]
        [Description("The unique identifier assigned to this appliance deployment.")]
        public string Id { get; set; } = "";

        [Category("1 - Identity")]
        [DisplayName("Alias")]
        [PropertySortOrder(2)]
        [Description("The alternate name for this appliance deployment. Useful when running 2 instances of the same appliance")]
        public string Alias { get; set; } = "";

        [Category("1 - Identity")]
        [DisplayName("Name")]
        [PropertySortOrder(3)]
        [Description("The name of the appliance.")]
        public string Name { get; set; } = "";

        [Category("1 - Identity")]
        [DisplayName("Build")]
        [PropertySortOrder(4)]
        [Description("The version of the appliance.")]
        public string Build { get; set; } = "";



        [Category("2 - Partial")]
        [DisplayName("Name")]
        [PropertySortOrder(5)]
        [Description("The name of the partial appliance. If one is being used.")]
        public string PartialName { get; set; } = "";

        [Category("2 - Partial")]
        [DisplayName("Build")]
        [PropertySortOrder(6)]
        [Description("The version of the partial appliance. If one is being used.")]
        public string PartialBuild { get; set; } = "";



        [Category("3 - Deployment")]
        [DisplayName("Location")]
        [PropertySortOrder(7)]
        [Description("Where is the appliance package located in the repository.")]
        public string DeploymentLocation { get; set; }

        [Category("3 - Deployment")]
        [DisplayName("Partial Location")]
        [PropertySortOrder(8)]
        [Description("Where is the partial appliance package located in the repository.")]
        public string PartialDeploymentLocation { get; set; }



        [Category("4 - Execution")]
        [DisplayName("Configuration")]
        [PropertySortOrder(9)]
        [Description("What configuration is this appliance using?")]
        public string Environment { get; set; } = "";

        [Category("4 - Execution")]
        [DisplayName("Type")]
        [PropertySortOrder(10)]
        [Description("What type of execution is this appliance using. Full or Core")]
        public string ExecutionType { get; set; } = "";

        [Category("4 - Execution")]
        [DisplayName("Listening Port")]
        [PropertySortOrder(11)]
        [Description("The listening port that Kestrel will listen on.")]
        public int ListeningPort { get; set; }



        [Category("5 - Status")]
        [DisplayName("Deployed")]
        [PropertySortOrder(12)]
        [Description( "Has the appliance successfully deployed? Downloaded from repository and installed." )]
        public bool HasDeployed { get; set; } = false;

        [Category("5 - Status")]
        [DisplayName("Started")]
        [PropertySortOrder(13)]
        [Description("Has the appliance successfully started?")]
        public bool HasStarted { get; set; } = false;

        [Category("5 - Status")]
        [DisplayName("Stopped")]
        [PropertySortOrder(14)]
        [Description("Has the appliance successfully stopped?")]
        public bool HasStopped { get; set; } = false;


    }


}

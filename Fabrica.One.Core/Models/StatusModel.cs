﻿using System.ComponentModel;
using System.Text.Json.Nodes;
using Fabrica.Utilities.Types;

namespace Fabrica.One.Models
{


    [TypeConverter(typeof(PropertySorterConverter))]
    public class StatusModel
    {


        [Category("1 - Identity")]
        [DisplayName("Uid")]
        [PropertySortOrder(1)]
        [Description("The unique identifier assigned to this appliance instance.")]
        public string Uid { get; set; } = "";

        [Category("1 - Identity")]
        [DisplayName("Alias")]
        [PropertySortOrder(2)]
        [Description("The alternate name for this appliance instance. Useful when running multiple instances of the same appliance")]
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

        [Category("1 - Identity")]
        [DisplayName("Checksum")]
        [PropertySortOrder(4)]
        [Description("The SHA256 checksum of the repository contents.")]
        public string Checksum { get; set; } = "";


        [Category("2 - Location")]
        [DisplayName("Location")]
        [PropertySortOrder(5)]
        [Description("Where is the appliance package located in the repository.")]
        public string RepositoryLocation { get; set; }

        [Category("2 - Location")]
        [DisplayName("Location")]
        [PropertySortOrder(6)]
        [Description("Where is the appliance instance installed.")]
        public string InstallationLocation { get; set; }


        [Category("3 - Execution")]
        [DisplayName("Startup Assembly")]
        [PropertySortOrder(8)]
        [Description("What is the name of the startup assembly?")]
        public string Assembly { get; set; } = "";


        [Category("4 - Status")]
        [DisplayName("Has Loaded")]
        [PropertySortOrder(11)]
        [Description( "Has the appliance successfully loaded? Downloaded from repository." )]
        public bool HasLoaded { get; set; } = false;

        [Category("4 - Status")]
        [DisplayName("Has Installed")]
        [PropertySortOrder(12)]
        [Description("Has the appliance successfully installed? Extracted from zip file.")]
        public bool HasInstalled { get; set; } = false;

        [Category("4 - Status")]
        [DisplayName("Has Started")]
        [PropertySortOrder(13)]
        [Description("Has the appliance successfully started?")]
        public bool HasStarted { get; set; } = false;

        [Category("4 - Status")]
        [DisplayName("Has Stopped")]
        [PropertySortOrder(14)]
        [Description("Has the appliance successfully stopped?")]
        public bool HasStopped { get; set; } = false;


        [Browsable(false)] 
        public JsonObject EnvironmentConfiguration { get; set; } = new JsonObject();

        public override string ToString()
        {
            return $"{Name}-{Build} running as {Alias} [Loaded: {HasLoaded}, Installed: {HasInstalled}, Started: {HasStarted}, Stopped: {HasStopped} ]";
        }

    }


}

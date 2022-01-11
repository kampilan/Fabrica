﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Fabrica.One.Models
{

    
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class DeploymentModel: BaseMutableModel<DeploymentModel>, IAggregateModel, INotifyPropertyChanged
    {

        private JsonSerializerOptions Options { get; } = new JsonSerializerOptions {WriteIndented = true};


        private long _id;
        [JsonIgnore]
        [Editable(false)]
        public override long Id
        {
            get => _id;
            protected set => _id = value;
        }

        private string _uid = Base62Converter.NewGuid();
        [JsonIgnore]
        [Editable(false)]
        public override string Uid
        {
            get => _uid;
            set => _uid = value;
        }

        private MissionModel _parent;
        [JsonIgnore]
        [Editable(false)]
        public MissionModel Parent
        {
            get => _parent;
            set => _parent = value;
        }

        public void SetParent(object parent)
        {

            if (parent is MissionModel mm)
                _parent = mm;

        }


        private string _name = "";
        [Required]
        public string Name
        {
            get=>_name;
            set=>_name = value;
        }

        private string _alias = "";
        [Required]
        public string Alias
        {
            get => _alias;
            set => _alias = value;
        }

        private string _build = "";
        [Required]
        public string Build
        {
            get => _build;
            set => _build = value;
        }

        private string _checksum = "";
        [Required]
        public string Checksum
        {
            get => _checksum;
            set => _checksum = value;
        }


        private string _environment = "";
        [Required]
        public string Environment
        {
            get => _environment;
            set => _environment = value;
        }


        private string _assembly = "";
        [Required]
        public string Assembly
        {
            get => _assembly;
            set => _assembly = value;
        }


        private bool _deploy;
        [Required]
        public bool Deploy
        {
            get => _deploy;
            set => _deploy = value;
        }


        private bool _waitForStart;
        [Required]
        public bool WaitForStart
        {
            get => _waitForStart;
            set => _waitForStart = value;
        }


        private bool _showWindow;
        [Required]
        public bool ShowWindow
        {
            get => _showWindow;
            set => _showWindow = value;
        }

        private JsonObject _environmentConfiguration = new JsonObject();
        [Editable(false)]
        public JsonObject EnvironmentConfiguration
        {
            get => _environmentConfiguration;
            set => _environmentConfiguration = value;
        }

        public string GetConfigurationAsJson() => JsonSerializer.Serialize(EnvironmentConfiguration, Options);

        public void SetConfiguration(string value)
        {
            var jo = JsonNode.Parse(value);
            EnvironmentConfiguration = jo?.AsObject() ?? new JsonObject();
        }

        public void SetConfiguration( Dictionary<string,object> config )
        {
            var json = JsonSerializer.Serialize(config);
            SetConfiguration(json);
        }


        public void Apply( BuildModel build )
        {

            if( Name == build.Name )
            {
                Build = build.BuildNum;
                Checksum = build.Checksum;
                Assembly = build.Assembly;
            }

        }


        public override string ToString()
        {
            return $"{Name}-{Build} as {Alias}";
        }

    }

}
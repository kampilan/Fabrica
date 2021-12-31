using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Fabrica.One.Plan
{


    public class PlanImpl : IPlan
    {


        public string Name { get; set; } = "";
        public string Environment { get; set; } = "";


        public bool RealtimeLogging { get; set; } = false;
        public string WatchEventStoreUri { get; set; } = "";
        public string WatchDomainName { get; set; } = "";


        public string RepositoryRoot { get; set; } = "";
        public string InstallationRoot { get; set; } = "";
        public string ConfigurationType { get; set; } = "";

        public string TokenSigningKey { get; set; }


        public bool DeployAppliances { get; set; } = true;
        public bool StartAppliances { get; set; } = true;
        public bool StartInParallel { get; set; } = true;
        public bool AllAppliancesMustDeploy { get; set; } = true;


        public int WaitForDeploySeconds { get; set; } = 120;
        public int WaitForStartSeconds { get; set; } = 60;
        public int WaitForStopSeconds { get; set; } = 30;


        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();


        public List<DeploymentUnit> Deployments { get; set; } = new List<DeploymentUnit>();


        public virtual void Validate()
        {


            if( string.IsNullOrWhiteSpace(TokenSigningKey) )
            {

                var rng = RandomNumberGenerator.Create();
                var key = new byte[64];
                rng.GetNonZeroBytes(key);

                TokenSigningKey = Convert.ToBase64String(key);

            }


            if (string.IsNullOrWhiteSpace(Name))
                Name = "Shtarker";

            if (string.IsNullOrWhiteSpace(Environment))
                Environment = "Development";

            if (string.IsNullOrWhiteSpace(RepositoryRoot))
                RepositoryRoot = "fabrica-appliance-repository";

            if (string.IsNullOrWhiteSpace(InstallationRoot))
                InstallationRoot = $"{Path.DirectorySeparatorChar}appliances";

            if (string.IsNullOrWhiteSpace(ConfigurationType))
                ConfigurationType = "yml";


            if (Deployments.Count == 0)
            {

                var shtarker = new DeploymentUnit
                {
                    Name = "Shtarker"
                };

                Deployments.Add( shtarker );

            }



            foreach (var unit in Deployments)
            {


                if( unit.Deploy )
                    unit.Deploy = DeployAppliances;

                if (string.IsNullOrWhiteSpace(unit.RepositoryRoot))
                    unit.RepositoryRoot = RepositoryRoot;

                if (string.IsNullOrWhiteSpace(unit.InstallationRoot))
                    unit.InstallationRoot = InstallationRoot;

                if (string.IsNullOrWhiteSpace(unit.ConfigurationType))
                    unit.ConfigurationType = ConfigurationType;

                if (string.IsNullOrWhiteSpace(unit.Id))
                    unit.Id = Guid.NewGuid().ToString();

                if (string.IsNullOrWhiteSpace(unit.Alias))
                    unit.Alias = unit.Name;

                if (string.IsNullOrWhiteSpace(unit.Build))
                    unit.Build = "latest";

                if (string.IsNullOrWhiteSpace(unit.Assembly))
                    unit.Assembly = "Appliance";

                
                if( !string.IsNullOrWhiteSpace(unit.PartialName) && string.IsNullOrWhiteSpace(unit.PartialBuild) )
                    unit.PartialBuild = "latest";

                if( !string.IsNullOrWhiteSpace(unit.PartialName) && string.IsNullOrWhiteSpace(unit.PartialAssembly) )
                    unit.PartialAssembly = "Appliance";
               

                
                if (string.IsNullOrWhiteSpace(unit.EventNamePrefix))
                    unit.EventNamePrefix = $"Fabrica.Appliance.{unit.Id}";

                if (string.IsNullOrWhiteSpace(unit.Environment))
                    unit.Environment = Environment;

                if (unit.ListeningPort > 0)
                    unit.Configuration["ListeningPort"] = unit.ListeningPort;



                // *** Watch related configuration *****************************************

                if( RealtimeLogging )
                    unit.RealtimeLogging = true;

                unit.Configuration["RealtimeLogging"] = unit.RealtimeLogging;
               


                if( string.IsNullOrWhiteSpace(unit.WatchEventStoreUri) && !string.IsNullOrWhiteSpace(WatchEventStoreUri) )
                    unit.WatchEventStoreUri = WatchEventStoreUri;

                if( !string.IsNullOrWhiteSpace(unit.WatchEventStoreUri) )
                    unit.Configuration["WatchEventStoreUri"] = unit.WatchEventStoreUri;


                if( string.IsNullOrWhiteSpace(unit.WatchDomainName) && !string.IsNullOrWhiteSpace(WatchDomainName) )
                    unit.WatchDomainName = WatchDomainName;

                if( !string.IsNullOrWhiteSpace(unit.WatchDomainName) )
                    unit.Configuration["WatchDomainName"] = unit.WatchDomainName;




                // *************************************************************************
                if ( !unit.RequiresAuthentication )
                    unit.Configuration["RequiresAuthentication"] = false;


                // *** Repository related configuration ************************************

                if( string.IsNullOrWhiteSpace(unit.DeploymentLocation) )
                    unit.DeploymentLocation = $"appliances/{unit.Name}/builds/{unit.Name}-{unit.Build}.zip";

                if( !string.IsNullOrWhiteSpace(unit.PartialName) && string.IsNullOrWhiteSpace(unit.PartialDeploymentLocation) )
                    unit.PartialDeploymentLocation = $"appliances/{unit.PartialName}/builds/{unit.PartialName}-{unit.PartialBuild}.zip";

                if (string.IsNullOrWhiteSpace(unit.ConfigurationLocation))
                    unit.ConfigurationLocation = $"appliances/{unit.Name}/configurations/{unit.Environment.ToLowerInvariant()}.{unit.ConfigurationType}";

                // *************************************************************************



                // *** Execution related configuration *************************************


                if (string.IsNullOrWhiteSpace(unit.ExecutionType))
                    unit.ExecutionType = "Core";

                if (string.IsNullOrWhiteSpace(unit.ExecutionPath))
                    unit.ExecutionPath = $"{InstallationRoot}{Path.DirectorySeparatorChar}{unit.Alias}{Path.DirectorySeparatorChar}{unit.Id}";


                if (unit.ExecutionType == "Core")
                {

                    if (string.IsNullOrWhiteSpace(unit.ExecutionCommand))
                        unit.ExecutionCommand = "dotnet";

                    if (string.IsNullOrWhiteSpace(unit.ExecutionArguments))
                        unit.ExecutionArguments = $"{unit.ExecutionPath}/{unit.Assembly}.dll";

                }


                if (unit.ExecutionType == "Full")
                {

                    if (string.IsNullOrWhiteSpace(unit.ExecutionCommand))
                        unit.ExecutionCommand = $"{unit.ExecutionPath}/{unit.Assembly}.exe";

                    if (string.IsNullOrWhiteSpace(unit.ExecutionArguments))
                        unit.ExecutionArguments = "";

                }


                if (!string.IsNullOrWhiteSpace(unit.EventNamePrefix))
                    unit.Configuration["EventNamePrefix"] = unit.EventNamePrefix;

                if (!StartInParallel)
                    unit.WaitForStart = true;



                foreach (var pair in Configuration)
                {

                    if( unit.Configuration.ContainsKey(pair.Key) )
                        continue;

                    unit.Configuration.Add( pair.Key, pair.Value);

                }

                unit.Configuration["MissionName"]     = Name;
                unit.Configuration["TokenSigningKey"] = TokenSigningKey;
                unit.Configuration["ApplianceName"]   = unit.Name;
                unit.Configuration["ApplianceBuild"]  = unit.Build;
                unit.Configuration["Environment"]     = unit.Environment;

            }


        }


    }


}

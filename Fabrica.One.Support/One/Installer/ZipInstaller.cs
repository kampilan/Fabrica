using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Fabrica.One.Plan;
using Fabrica.Watch;
using JetBrains.Annotations;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fabrica.One.Installer
{


    public class ZipInstaller: IApplianceInstaller
    {


        public void Clean( IPlan plan )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to build directory info for installation root");
                var installation = new DirectoryInfo(plan.InstallationRoot);



                // *****************************************************************
                logger.Debug("Attempting to check if directory exists");
                if (installation.Exists)
                {
                    logger.Debug("Attempting to recursively delete installation directory");
                    installation.Delete(true);
                }


                // *****************************************************************
                logger.Debug("Attempting to create installation directory");
                installation.Create();


            }
            catch (Exception cause)
            {

                var message = $"Installation failed during Cleanup. Mission: {plan.Name} InstallationRoot: ({plan.InstallationRoot}) Message: {cause.Message}";

                logger.Error( cause, message );

                throw new Exception( message );

            }
            finally
            {
                logger.LeaveMethod();
            }


        }




        public async Task Install([NotNull] DeploymentUnit unit )
        {

            if (unit == null) throw new ArgumentNullException(nameof(unit));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                var deployment = "";
                try
                {

                    logger.Debug("Attempting to get deployment directory");
                    deployment = $"{unit.InstallationRoot}/{unit.Alias}/{unit.Id}";

                    logger.Inspect(nameof(deployment), deployment);



                    logger.Debug("Attempting to check if deployment dir exists");
                    var deployDir = new DirectoryInfo(deployment);
                    if (!deployDir.Exists)
                    {
                        logger.Debug("Attempting to create deployment directory");
                        deployDir.Create();
                    }

                }
                catch (Exception cause )
                {

                    var message = $"Appliance {unit.Name} failed while creating installation directory ({deployment}). Message: {cause.Message}";

                    logger.Error(cause, message);

                    throw new Exception(message);

                }



                // *****************************************************************
                logger.Debug("Attempting to check and install main deployment");
                await _checkAndInstall( $"{unit.Name}-{unit.Build}", unit.Checksum, deployment, unit.DeploymentContent );



                // *****************************************************************
                if (!string.IsNullOrWhiteSpace(unit.PartialName))
                {
                    logger.Debug("Attempting to check and install partial deployment");
                    await _checkAndInstall($"{unit.PartialName}-{unit.PartialBuild}", unit.PartialChecksum, deployment, unit.PartialDeploymentContent );
                }



                // *****************************************************************
                var configFile = "";
                try
                {


                    logger.Debug("Attempting to build environment config file path");
                    configFile = $"{deployment}/environment.{unit.ConfigurationType}";

                    logger.Inspect(nameof(configFile), configFile);



                    logger.Debug("Attempting to write config file");
                    using (var file = new FileStream(configFile, FileMode.Create, FileAccess.Write))
                        await unit.ConfigurationContent.CopyToAsync(file);


                }
                catch( Exception cause )
                {

                    var message = $"Appliance {unit.Name}-{unit.Build} failed during configuration content installation. Target: ({configFile}) Message: {cause.Message}";

                    logger.Error(cause, message);

                    throw new Exception(message);

                }



                // *****************************************************************
                logger.Debug("Attempting to check for mission configuration");
                if( unit.Configuration.Count > 0 )
                {


                    var overrideFile = "";
                    try
                    {


                        logger.Debug("Creating mission configuration file");

                        overrideFile = $"{deployment}/mission.{unit.ConfigurationType}";
                        logger.Inspect(nameof(overrideFile), overrideFile);


                        using (var file = new FileStream(overrideFile, FileMode.Create, FileAccess.Write))
                        using (var writer = new StreamWriter(file))
                        {

                            var output = unit.ConfigurationType == "json" ? _createJson(unit.Configuration) : _createYaml(unit.Configuration);
                            writer.Write(output);

                        }


                    }
                    catch (Exception cause)
                    {

                        var message = $"Appliance {unit.Name}-{unit.Build} failed during mission.json installation. Target: ({overrideFile}) Message: {cause.Message}";

                        logger.Error(cause, message);

                        throw new Exception(message);

                    }


                }



                unit.HasInstalled = true;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        private Task _checkAndInstall( string label, string checksum, string deployment, MemoryStream content )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to check is a checksum was defined");
                if (!string.IsNullOrWhiteSpace(checksum))
                {

                    logger.Debug("Checksum defined");


                    bool matched;
                    string calculated;
                    try
                    {

                        logger.Debug("Attempting to initial SHA1 managed");
                        var sha = SHA1.Create();
                        sha.Initialize();



                        logger.Debug("Attempting to calculate hash for deployment content");
                        var bytes = sha.ComputeHash(content);
                        content.Seek(0, SeekOrigin.Begin);



                        logger.Debug("Attempting to converting hash bytes to string");
                        calculated = Convert.ToBase64String(bytes);

                        matched = calculated == checksum;


                    }
                    catch (Exception cause)
                    {

                        var message = $"Appliance {label} failed while enforcing checksum: ({checksum}). Message: {cause.Message}";

                        logger.Error(cause, message);

                        throw new Exception( message, cause );

                    }


                    logger.Inspect(nameof(matched), matched);


                    if (!matched)
                        throw new Exception($" Invalid SHA1 hash for Appliance: {label}, Expecting: {checksum} but calculated ({calculated})");


                }



                // *****************************************************************
                try
                {

                    logger.Debug("Attempting to extract deployment content to directory");
                    using (var arc = new ZipArchive(content))
                        arc.ExtractToDirectory(deployment);

                }
                catch (Exception cause)
                {

                    var message = $"Appliance {label} failed during deployment content installation. Target: ({deployment}) Message: {cause.Message}";

                    logger.Error(cause, message);

                    throw new Exception(message);

                }


                return Task.CompletedTask;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        private string _createJson( Dictionary<string, object> config )
        {

            var json = JsonConvert.SerializeObject(config);

            return json;

        }


        private string _createYaml( Dictionary<string, object> config )
        {

            var builder = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var yaml = builder.Serialize(config);

            return yaml;

        }


    }


}

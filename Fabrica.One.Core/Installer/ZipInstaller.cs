using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Fabrica.One.Plan;
using Fabrica.Watch;
using JetBrains.Annotations;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Fabrica.One.Installer
{


    public class ZipInstaller : IApplianceInstaller
    {

        public Task Clean( [NotNull] IPlan plan )
        {

            if (plan == null) throw new ArgumentNullException(nameof(plan));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(plan.InstallationRoot), plan.InstallationRoot);



                // *****************************************************************
                logger.Debug("Attempting to check if directory exists");
                var installDir = new DirectoryInfo(plan.InstallationRoot);
                if( installDir.Exists )
                {
                    logger.Debug("Attempting to recursively delete installation directory");
                    installDir.Delete(true);
                }



                // *****************************************************************
                logger.Debug("Attempting to create installation directory");
                installDir.Create();



                // *****************************************************************
                return Task.CompletedTask;


            }
            catch (Exception cause)
            {

                var message = $"Installation failed during Cleanup. Mission: {plan.Name} InstallationRoot: ({plan.InstallationRoot}) Message: {cause.Message}";

                logger.Error(cause, message);

                throw;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }




        public async Task Install([NotNull] IPlan plan, [NotNull] DeploymentUnit unit)
        {

            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (unit == null) throw new ArgumentNullException(nameof(unit));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(unit.InstallationLocation), unit.InstallationLocation);
                logger.Inspect(nameof(unit.EnvironmentConfigLocation), unit.EnvironmentConfigLocation);
                logger.Inspect(nameof(unit.MissionConfigLocation), unit.MissionConfigLocation);
                logger.Inspect(nameof(unit.RepositoryContent.Length), unit.RepositoryContent.Length);
                logger.Inspect(nameof(unit.Deploy), unit.Deploy);



                // *****************************************************************
                if (!unit.Deploy)
                {
                    logger.Debug("Skipping deploy per Unit Deploy flag=false");
                    return;
                }


                // *****************************************************************
                if( unit.RepositoryContent.Length == 0 )
                {
                    logger.Debug("Skipping deploy per Unit RepositoryContent length=0");
                    return;
                }



                // *****************************************************************
                try
                {

                    logger.Debug("Attempting to check if installation dir exists");
                    var installDir = new DirectoryInfo( unit.InstallationLocation );
                    if( !installDir.Exists )
                    {
                        logger.Debug("Attempting to create installation directory");
                        installDir.Create();
                    }

                }
                catch (Exception cause)
                {

                    var message = $"Appliance {unit.Alias} failed while creating installation directory ({unit.InstallationLocation}). Message: {cause.Message}";

                    logger.Error(cause, message);

                    throw;

                }



                // *****************************************************************
                try
                {

                    logger.Debug("Attempting to extract repository content");
                    using (var arc = new ZipArchive(unit.RepositoryContent, ZipArchiveMode.Read, true) )
                        arc.ExtractToDirectory(unit.InstallationLocation);



                    logger.Debug("Attempting to release repository content");
                    unit.RepositoryContent.SetLength(0);


                }
                catch (Exception cause)
                {

                    var ctx = new
                    {
                        unit.Alias, unit.Build, unit.InstallationLocation, ContentLength = unit.RepositoryContent.Length, ExceptionType = cause.GetType().FullName, cause.Message
                    };

                    logger.ErrorWithContext(cause, ctx, $"Appliance ({unit.Alias}) Build ({unit.Build}) failed during repository content installation.");

                    throw;

                }



                // *****************************************************************
                try
                {


                    logger.Debug("Attempting to serialize environment confguration to JSON");
                    var json = unit.EnvironmentConfiguration.ToString();
                    logger.LogJson("Unit EnvironmentConfiguration", json);


                    logger.Debug("Attempting to write environment config file");
                    using (var file = new FileStream(unit.EnvironmentConfigLocation, FileMode.Create, FileAccess.Write))
                    using (var writer = new StreamWriter(file))
                    {
                        await writer.WriteAsync(json);
                        await writer.FlushAsync();
                    }


                }
                catch (Exception cause)
                {

                    var message = $"Appliance {unit.Name}-{unit.Build} failed during Environment Config installation. Target: ({unit.EnvironmentConfigLocation}) Message: {cause.Message}";

                    logger.Error(cause, message);

                    throw;

                }



                // *****************************************************************
                try
                {


                    logger.Debug("Attempting to serialize mission confguration to JSON");

                    var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
                    {
                        WriteIndented = true
                    };

                    var json = JsonSerializer.Serialize(unit.MissionConfiguration, options);
                    logger.LogJson("Mission EnvironmentConfiguration", json);


                    logger.Debug("Attempting to write mission config file");
                    using (var file = new FileStream(unit.MissionConfigLocation, FileMode.Create, FileAccess.Write))
                    using (var writer = new StreamWriter(file))
                    {
                        await writer.WriteAsync(json);
                        await writer.FlushAsync();
                    }


                }
                catch (Exception cause)
                {

                    var message = $"Appliance {unit.Name}-{unit.Build} failed during Mission Config installation. Target: ({unit.MissionConfigLocation}) Message: {cause.Message}";

                    logger.Error(cause, message);

                    throw;

                }


                unit.HasInstalled = true;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

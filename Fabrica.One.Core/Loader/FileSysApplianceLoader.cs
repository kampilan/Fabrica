using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Fabrica.One.Plan;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.One.Loader
{


    public class FileSysApplianceLoader: IApplianceLoader
    {


        public Task Clean( IPlan plan )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(plan.RepositoryRoot), plan.RepositoryRoot);
                logger.Inspect(nameof(plan.RepositoryVersion), plan.RepositoryVersion);



                // *****************************************************************
                logger.Debug("Attempting to check and build RepositoryRoot");
                var repoDir = new DirectoryInfo(plan.RepositoryRoot);
                if( !repoDir.Exists )
                    repoDir.Create();



                // *****************************************************************
                logger.Debug("Attempting to delete all repository versions except for the one in the current plan");
                foreach (var rv in repoDir.EnumerateDirectories())
                {

                    logger.Inspect(nameof(rv.FullName), rv.FullName);

                    if( rv.Name != plan.RepositoryVersion )
                        rv.Delete(true);

                }



                // *****************************************************************
                return Task.CompletedTask;


            }
            catch (Exception cause)
            {

                var message = $"Loading failed during Cleanup. Mission: {plan.Name} RepositoryRoot: ({plan.RepositoryRoot}) Message: {cause.Message}";

                logger.Error(cause, message);

                throw;

            }

            finally
            {
                logger.LeaveMethod();
            }


        }


        public async Task Load([NotNull] IPlan plan, [NotNull] DeploymentUnit unit)
        {

            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (unit == null) throw new ArgumentNullException(nameof(unit));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(unit.RepositoryLocation), unit.RepositoryLocation);
                logger.Inspect(nameof(unit.Deploy), unit.Deploy);


                // *****************************************************************
                if( !unit.Deploy )
                {
                    logger.Debug("Skipping deploy per Unit Deploy flag=false");
                    return;
                }



                //****************************************
                try
                {

                    logger.Debug("Attempting to load build");


                    using( var source = new FileStream(unit.RepositoryLocation, FileMode.Open, FileAccess.Read) )
                        await source.CopyToAsync(unit.RepositoryContent);

                    unit.RepositoryContent.Seek(0, SeekOrigin.Begin);



                    // *****************************************************************
                    if( !string.IsNullOrWhiteSpace(unit.Checksum) )
                    {

                        logger.Debug("Attempting to verify repository content");

                        bool matched;
                        string calculated;
                        try
                        {

                            logger.Debug("Attempting to initial SHA256 managed");
                            var sha = SHA256.Create();
                            sha.Initialize();



                            logger.Debug("Attempting to calculate hash for repository content");
                            var bytes = sha.ComputeHash(unit.RepositoryContent);
                            unit.RepositoryContent.Seek(0, SeekOrigin.Begin);



                            logger.Debug("Attempting to converting hash bytes to string");
                            calculated = bytes.ToHexString();

                            matched = calculated == unit.Checksum;


                        }
                        catch (Exception cause)
                        {

                            var message = $"Appliance {unit.Alias} failed while verifying checksum: ({unit.Checksum}). Message: {cause.Message}";

                            logger.Error(cause, message);

                            throw;

                        }


                        logger.Inspect(nameof(matched), matched);

                        if( !matched )
                        {
                            unit.RepositoryContent.SetLength(0);
                            throw new Exception($" Invalid SHA256 hash for Appliance: {unit.Alias}, Expecting: ({unit.Checksum}) but calculated ({calculated})");
                        }

                    }



                    unit.HasLoaded = true;


                }
                catch (Exception cause)
                {

                    var message = $"Appliance ({unit.Alias}) failed while getting repository content.  Repository Location: ({unit.RepositoryLocation}) Message: {cause.Message}";
                    logger.Error(cause, message);

                    throw;

                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }



}

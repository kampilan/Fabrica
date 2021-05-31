using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.One.Plan;
using Fabrica.Utilities.Storage;
using Fabrica.Watch;

namespace Fabrica.One.Loader
{

    public class StorageLoader: IApplianceLoader
    {

        public StorageLoader( IStorageProvider storage )
        {

            Storage = storage;

        }


        private IStorageProvider Storage { get; }


        public async Task Load( DeploymentUnit unit )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                //****************************************
                try
                {

                    logger.Debug("Attempting to load build");


                    using( var strm = new MemoryStream() )
                    {
                        await Storage.GetAsync(unit.RepositoryRoot, unit.DeploymentLocation, strm);
                        await strm.CopyToAsync(unit.DeploymentContent);
                    }

                    unit.DeploymentContent.Seek(0, SeekOrigin.Begin);


                }
                catch (Exception cause)
                {

                    var message = $"Appliance ({unit.Name}) failed while getting deployment content. Loader: {GetType().FullName}  RepositoryRoot: ({unit.RepositoryRoot}) Location: ({unit.DeploymentLocation}) Message: {cause.Message}";
                    logger.Error( cause, message );

                    throw new Exception( message );

                }



                //****************************************
                try
                {


                    if( !string.IsNullOrWhiteSpace(unit.PartialDeploymentLocation) )
                    {

                        logger.Debug("Attempting to load partial build");

                        using (var strm = new MemoryStream())
                        {
                            await Storage.GetAsync(unit.RepositoryRoot, unit.PartialDeploymentLocation, strm);
                            await strm.CopyToAsync(unit.PartialDeploymentContent);
                        }

                        unit.PartialDeploymentContent.Seek(0, SeekOrigin.Begin);

                    }


                }
                catch (Exception cause)
                {

                    var message = $"Appliance ({unit.PartialName}) failed while getting deployment content. Loader: {GetType().FullName}  RepositoryRoot: ({unit.RepositoryRoot}) Location: ({unit.PartialDeploymentLocation}) Message: {cause.Message}";
                    logger.Error(cause, message);

                    throw new Exception(message);

                }



                //****************************************
                try
                {

                    logger.Debug("Attempting to load environment configuration");

                    using (var strm = new MemoryStream())
                    {
                        await Storage.GetAsync(unit.RepositoryRoot, unit.ConfigurationLocation, strm);
                        await strm.CopyToAsync(unit.ConfigurationContent);
                    }


                    unit.ConfigurationContent.Seek(0, SeekOrigin.Begin);

                }
                catch (Exception cause)
                {

                    var message = $"Appliance ({unit.Name}) failed while getting configuration content. Loader: {GetType().FullName}  RepositoryRoot: ({unit.RepositoryRoot}) Location: ({unit.ConfigurationLocation}) Message: {cause.Message}";
                    logger.Error(cause, message);

                    throw new Exception(message);

                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }

}

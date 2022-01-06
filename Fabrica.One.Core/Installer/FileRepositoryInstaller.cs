using System.IO;
using System.Threading.Tasks;
using Fabrica.One.Plan;
using Fabrica.Watch;

namespace Fabrica.One.Installer
{


    public class FileRepositoryInstaller: IApplianceInstaller
    {

        public Task Clean( IPlan plan )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to check for RepositoryVersion directory");
                var repoDir = new DirectoryInfo(Path.Combine(plan.RepositoryRoot, plan.RepositoryVersion));
                if( repoDir.Exists )
                {
                    logger.Debug("Repository version exists. Deleting");
                    repoDir.Delete(true);
                }



                // *****************************************************************
                logger.Debug("Attempting to creating Repository version directory");
                repoDir.Create();


                return Task.CompletedTask;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public async Task Install(IPlan plan, DeploymentUnit unit)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to save RepositoryContent to repository");

                using (var fo = new FileStream(unit.RepositoryLocation, FileMode.Create, FileAccess.Write))
                    await unit.RepositoryContent.CopyToAsync(fo);



                // *****************************************************************
                logger.Debug("Attempting to release Repository");
                unit.RepositoryContent.SetLength(0);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

    }


}

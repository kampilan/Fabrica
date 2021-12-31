using System.IO;
using System.Threading.Tasks;
using Fabrica.Watch;

namespace Fabrica.One.Plan
{


    public class FilePlanSource: AbstractPlanSource, IPlanSource
    {


        public string FilePath { get; set; } = "local.mission.yml";
        
        
        public async Task<Stream> GetSource()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(FilePath), FilePath);


                // *****************************************************************
                logger.Debug("Attempting to get FileInfo");
                CurrentPlan = new FileInfo(FilePath);



                // *****************************************************************
                logger.Debug("Attempting to load plan file into stream");
                var stream = new MemoryStream();
                using (var fs = new FileStream(CurrentPlan.FullName, FileMode.Open, FileAccess.Read))
                {
                    await fs.CopyToAsync(stream);
                }

                stream.Seek(0, SeekOrigin.Begin);



                // *****************************************************************
                return stream;


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        private FileInfo CurrentPlan { get; set; }

        protected override Task<bool> CheckForUpdate()
        {

            var fi = new FileInfo(FilePath);

            var updated = fi.LastWriteTime > CurrentPlan.LastWriteTime;

            return Task.FromResult(updated);

        }


    }


}

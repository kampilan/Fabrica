using System.IO;
using System.Threading.Tasks;
using Fabrica.Watch;

namespace Fabrica.One.Plan
{


    public class FilePlanWriter: IPlanWriter
    {


        public string MissionFileDir { get; set; } = "";
        public string MissionFileName { get; set; } = "mission-plan.json";

        public async Task Write( string missionPlanJson )
        {
            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to calculate mission plan destination");
                var dest = $"{MissionFileDir}{Path.DirectorySeparatorChar}{MissionFileName}";
                logger.Inspect(nameof(dest), dest);



                // *****************************************************************
                logger.Debug("Attempting to write mission plan JSON");
                using( var fo = new FileStream( dest, FileMode.Create, FileAccess.Write) )
                using( var writer = new StreamWriter(fo) )
                {
                    await writer.WriteAsync(missionPlanJson);
                    await writer.FlushAsync();
                }


            }
            finally
            {
                logger.LeaveMethod();
            }



        }

    }

}

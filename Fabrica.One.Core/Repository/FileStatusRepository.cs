using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Fabrica.One.Models;
using Fabrica.Watch;

namespace Fabrica.One.Repository
{

    public class FileStatusRepository: IStatusRepository
    {

        private class Status
        {
            public Result Results { get; set; } = new Result();
            public List<ApplianceModel> Appliances { get; set; } = new List<ApplianceModel>();

        }


        public string OneRoot { get; set; } = "";

        public async Task<IEnumerable<ApplianceModel>> GetAppliances()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(OneRoot), OneRoot);



                // *****************************************************************
                logger.Debug("Attempting to build path to status file");
                var path = $"{OneRoot}{Path.DirectorySeparatorChar}mission-status.json";
                
                logger.Inspect(nameof(path), path);



                // *****************************************************************
                logger.Debug("Attempting to check for status file existence");
                var file = new FileInfo(path);
                if (!file.Exists)
                {
                    logger.Debug("Status file does not exist");
                    return new List<ApplianceModel>();
                }


                // *****************************************************************
                logger.Debug("Attempting to read file");
                using( var fi = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                using( var reader = new StreamReader(fi) )
                {

                    var json = await reader.ReadToEndAsync();
                    logger.LogJson("Mission Status JSON", json, false);


                    // *****************************************************************
                    logger.Debug("Attempting to deserialize json");
                    var status = JsonSerializer.Deserialize<Status>(json, new JsonSerializerOptions(JsonSerializerDefaults.General));
                    var apps = status?.Appliances??new List<ApplianceModel>();


                    // *****************************************************************
                    return apps;

                }

            }
            finally
            {
                logger.LeaveMethod();
            }


        }

    }


}

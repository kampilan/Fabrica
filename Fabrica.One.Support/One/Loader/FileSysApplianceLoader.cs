using System.IO;
using System.Threading.Tasks;
using Fabrica.Watch;

namespace Fabrica.One.Loader
{


    public class FileSysApplianceLoader: AbstractApplianceLoader
    {


        public string LocalPath { get; set; } = "c:/repository";


        protected override async Task Fetch( string root, string key, MemoryStream stream )
        {

            var logger = this.GetLogger();

            var path = "";
            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to build full path ");
                path = Path.Combine( LocalPath, root, key );
                logger.Inspect(nameof(path), path);



                // *****************************************************************
                logger.Debug("Attempting to load requested file");
                using( var strm = new FileStream(path, FileMode.Open, FileAccess.Read) )
                    await strm.CopyToAsync(stream);

                stream.Seek(0, SeekOrigin.Begin);



            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }



}

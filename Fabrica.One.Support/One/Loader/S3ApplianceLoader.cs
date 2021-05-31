using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Watch;

namespace Fabrica.One.Loader
{
    
    public class S3ApplianceLoader: AbstractApplianceLoader
    {


        public S3ApplianceLoader(IAmazonS3 client)
        {

            Client = client;

        }

        private IAmazonS3 Client { get; }
        
        protected override async Task Fetch(string root, string key, MemoryStream content )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect("root", root);
                logger.Inspect("key", key);


                // *********************************************************************
                logger.Debug("Attempting to build S3 GetObjectRequest");
                var request = new GetObjectRequest
                {
                    BucketName = root,
                    Key        = key
                };



                // *********************************************************************
                logger.Debug("Attempting to call GetObject from S3");
                var response = await Client.GetObjectAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    logger.LogObject("response", response);
                    throw new IOException($"Could not Get item using Root=({root}) Key=({key})");
                }



                // *****************************************************************
                logger.Debug("Attempting to copy response stream to content");
                using (var stream = response.ResponseStream)
                {
                    await stream.CopyToAsync(content);
                    content.Seek(0, SeekOrigin.Begin);
                }


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


    }


}

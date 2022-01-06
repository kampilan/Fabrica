using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.One.Loader;
using Fabrica.One.Plan;
using Fabrica.Watch;

namespace Fabrica.One.Orchestrator.Aws.Loader
{

    
    public class S3ApplianceLoader: IApplianceLoader
    {


        public S3ApplianceLoader( IAmazonS3 client, string bucketName )
        {

            Client     = client;
            BucketName = bucketName;
        }


        private IAmazonS3 Client { get; }
        private string BucketName { get; }

        public Task Clean( IPlan plan )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                return Task.CompletedTask;


            }
            finally
            {
                logger.LeaveMethod();
            }
            
        }

        public async Task Load( IPlan plan,DeploymentUnit unit )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var key = "";
                //****************************************
                try
                {

                    logger.Debug("Attempting to load build");

                    key = $"appliances/{unit.Name}/{unit.Name}-{unit.Build}.zip";


                    // *********************************************************************
                    logger.Debug("Attempting to build S3 GetObjectRequest");
                    var request = new GetObjectRequest
                    {
                        BucketName = BucketName,
                        Key = key
                    };



                    // *********************************************************************
                    logger.Debug("Attempting to call GetObject from S3");
                    var response = await Client.GetObjectAsync(request);

                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        logger.LogObject("response", response);
                        throw new IOException($"Could not Get item using BucketName=({BucketName}) Key=({key})");
                    }



                    // *****************************************************************
                    logger.Debug("Attempting to copy response stream to content");
                    using (var stream = response.ResponseStream)
                    {
                        await stream.CopyToAsync(unit.RepositoryContent);
                        unit.RepositoryContent.Seek(0, SeekOrigin.Begin);
                    }

                    unit.RepositoryContent.Seek(0, SeekOrigin.Begin);

                    unit.HasLoaded = true;


                }
                catch (Exception cause)
                {

                    var message = $"Appliance ({unit.Name}) failed while getting repository content. Loader: {GetType().FullName}  RepositoryRoot: ({BucketName}) Location: ({key}) Message: {cause.Message}";
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

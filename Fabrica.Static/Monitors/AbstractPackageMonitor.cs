using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Static.Providers.Mutable;
using Fabrica.Utilities.Threading;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Static.Monitors
{


    public abstract class AbstractPackageMonitor: IPackageMonitorModule
    {


        protected AbstractPackageMonitor( IPackageMonitorModule config,  IAmazonS3 client, MutableDirectoryFileProvider provider)
        {

            Client = client;
            Provider = provider;

            LocalInstallationPath = config.LocalInstallationPath;

        }


        private IAmazonS3 Client { get; }
        private MutableDirectoryFileProvider Provider { get; }

        private ConcurrentQueue<string> OldRoots { get; } = new ConcurrentQueue<string>();

        public string LocalInstallationPath { get; set; }
        public string DeploymentName { get; set; }
        public int DeploymentMonitorIntervalSecs { get; set; } = 10;


        public virtual void Start()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var info = AsyncPump.Run(async ()=> await GetDeploymentInfo() );

                logger.LogObject(nameof(info), info);


                Deploy( info );


                MonitorTask = Task.Run( Monitor );


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public virtual void Dispose()
        {

            if( MonitorTask != null )
            {

                MustStop.Set();
                Stopped.WaitOne(TimeSpan.FromSeconds(10));

                Directory.Delete(LocalInstallationPath, true);

            }
            else
            {
                MustStop.Set();
                Stopped.Set();
            }

        }


        #region Monitoring and Deployment members


        protected abstract Task<IDeploymentInfo> GetDeploymentInfo();

        protected Task MonitorTask { get; private set; }
        protected ManualResetEvent MustStop { get; } = new ManualResetEvent(false);
        protected ManualResetEvent Stopped { get; } = new ManualResetEvent(false);

        private string LastPackageId { get; set; } = "";
        private DateTime LastTimestamp { get; set; } = new DateTime(1883, 11, 19, 0, 0, 0, 0);

        protected async Task Monitor()
        {

            var interval = TimeSpan.FromSeconds(DeploymentMonitorIntervalSecs);
            while( !MustStop.WaitOne(interval) )
            {


                var logger = this.GetLogger();


                if ( !OldRoots.IsEmpty )
                {

                    var oldRoot = "";
                    try
                    {

                        if( OldRoots.TryDequeue(out oldRoot) )
                            Directory.Delete(oldRoot, true);

                    }
                    catch (Exception cause)
                    {
                        logger.WarningFormat( cause, "Removal of old root failed, ({0})", oldRoot );
                    }

                }


                try
                {

                    var info = await GetDeploymentInfo();

                    if (info == null)
                    {
                        logger.Warning("Encountered null DeploymentInfo");
                        continue;
                    }

                    if (info.PackageLocation != LastPackageId || info.BuildDate > LastTimestamp)
                    {

                        await DeployAsync(info);

                        LastPackageId = info.PackageLocation;
                        LastTimestamp = info.BuildDate;

                    }


                }
                catch ( Exception cause )
                {
                    logger.Warning( cause, "GetDeploymentInfo failed" );
                }

                

            }

            Stopped.Set();

        }


        protected async Task DeployAsync( IDeploymentInfo info )
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var installPath = $"{LocalInstallationPath}{Path.DirectorySeparatorChar}package{Path.DirectorySeparatorChar}{ShortGuid.NewGuid()}";

                logger.Inspect(nameof(installPath), installPath);



                // *****************************************************************
                logger.Debug("Attempting to create request");
                var request = CreateRequest(info);



                // *****************************************************************
                logger.Debug("Attempting to send request");
                var response = await Client.GetObjectAsync(request);

                logger.LogObject(nameof(response.HttpStatusCode), response.HttpStatusCode);



                // *****************************************************************
                logger.Debug("Attempting to process response");
                ProcessResponse(response.ResponseStream, installPath);



                // *****************************************************************
                logger.Debug("Attempting to queue old root for removal");
                var oldRoot = Provider.Root;
                if (!string.IsNullOrWhiteSpace(oldRoot))
                    OldRoots.Enqueue(oldRoot);


                // *****************************************************************
                logger.Debug("Attempting to update root on FileProvider");
                Provider.SetRoot(installPath);

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected void Deploy( IDeploymentInfo info )
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var installPath = $"{LocalInstallationPath}{Path.DirectorySeparatorChar}package{Path.DirectorySeparatorChar}{ShortGuid.NewGuid()}";

                logger.Inspect(nameof(installPath), installPath);



                // *****************************************************************
                logger.Debug("Attempting to create request");
                var request = CreateRequest(info);



                // *****************************************************************
                logger.Debug("Attempting to send request");
                var response = AsyncPump.Run(async () => await Client.GetObjectAsync(request));

                logger.LogObject(nameof(response.HttpStatusCode), response.HttpStatusCode);



                // *****************************************************************
                logger.Debug("Attempting to process response");
                ProcessResponse(response.ResponseStream, installPath);



                // *****************************************************************
                logger.Debug("Attempting to queue old root for removal");
                var oldRoot = Provider.Root;
                if (!string.IsNullOrWhiteSpace(oldRoot))
                    OldRoots.Enqueue(oldRoot);



                // *****************************************************************
                logger.Debug("Attempting to update root on FileProvider");
                Provider.SetRoot(installPath);


            }
            finally
            {
                logger.LeaveMethod();
            }




        }


        private GetObjectRequest CreateRequest( IDeploymentInfo info )
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var request = new GetObjectRequest
                {
                    BucketName = info.PackageRepository,
                    Key = info.PackageLocation
                };


                return request;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        private void ProcessResponse( Stream content, string destination )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(destination), destination);

                using (var arc = new ZipArchive(content, ZipArchiveMode.Read, false))
                    arc.ExtractToDirectory(destination);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        #endregion



    }




}

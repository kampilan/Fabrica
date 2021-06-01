using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Autofac;
using Fabrica.Static.Providers.Mutable;
using Fabrica.Watch;

namespace Fabrica.Static.Monitors
{

    

    public class DynamoDbPackageMonitor : AbstractPackageMonitor, IStartable, IDisposable
    {


        [DynamoDBTable("deployments")]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class DeploymentInfo : IDeploymentInfo
        {

            [DynamoDBHashKey]
            public string Name { get; set; } = "";

            public string PackageRepository { get; set; } = "";
            public string PackageLocation   { get; set; } = "";

            public DateTime BuildDate { get; set; } = DateTime.Now;

        }


        public DynamoDbPackageMonitor( IAmazonDynamoDB dynamoDb,  IDynamoDbMonitorModule config, IAmazonS3 client, MutableDirectoryFileProvider provider) : base(config, client, provider)
        {
            
            DynamoDb  = dynamoDb;

            TableNamePrefix = config.TableNamePrefix;

        }


        private IAmazonDynamoDB DynamoDb { get; }

        public string TableNamePrefix { get; set; }


        protected override async Task<IDeploymentInfo> GetDeploymentInfo()
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(LocalInstallationPath), LocalInstallationPath);
                logger.Inspect(nameof(DeploymentName), DeploymentName);
                logger.Inspect(nameof(TableNamePrefix), TableNamePrefix);


                using( var ctx = new DynamoDBContext(DynamoDb) )
                {


                    var cfg = new DynamoDBContextConfig();

                    if( !string.IsNullOrWhiteSpace(TableNamePrefix) )
                        cfg.TableNamePrefix = TableNamePrefix;



                    // *****************************************************************
                    logger.Debug("Attempting to load deployment info from DynamoDB");
                    var info = await ctx.LoadAsync<DeploymentInfo>(DeploymentName, cfg );

                    logger.LogObject(nameof(info), info);



                    // *****************************************************************
                    return info;

                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

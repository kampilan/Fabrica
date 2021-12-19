using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Amazon.S3;
using Autofac;
using Fabrica.Static.Providers.Mutable;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace Fabrica.Static.Monitors
{


    public class MongoDbPackageMonitor : AbstractPackageMonitor, IRequiresStart, IDisposable
    {


        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class DeploymentInfo: IDeploymentInfo
        {

            public ObjectId Id { get; set; } = ObjectId.Empty;
        
            public string Name { get; set; } = "";

            public string PackageRepository { get; set; } = "";
            public string PackageLocation   { get; set; } = "";

            public DateTime BuildDate { get; set; } = DateTime.Now;

            [BsonExtraElements]
            public IDictionary<string,object> Extensions { get; } = new Dictionary<string, object>();


        }

        public MongoDbPackageMonitor( IMongoDbMonitorModule config, IAmazonS3 client, MutableDirectoryFileProvider provider) : base( config,client, provider )
        {

            MongoDbServerUri = config.MongoDbServerUri;
            MongoDbDatabase  = config.MongoDbDatabase;

            DeploymentName = config.DeploymentName;

        }


        public string MongoDbServerUri { get; set; }
        public string MongoDbDatabase { get; set; }


        private MongoClient Client { get; set; }
        private IMongoDatabase Database { get; set; }
        private IMongoCollection<DeploymentInfo> Collection { get; set; }

        public override async Task Start()
        {

            Client     = new MongoClient( MongoDbServerUri );
            Database   = Client.GetDatabase( MongoDbDatabase );
            Collection = Database.GetCollection<DeploymentInfo>( "Deployments" );

            await base.Start();

        }


        protected override async Task<IDeploymentInfo> GetDeploymentInfo()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to built retry policy");
                var delay = Backoff.AwsDecorrelatedJitterBackoff(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1), retryCount: 5);

                var policy = Policy
                    .Handle<MongoExecutionTimeoutException>()
                    .WaitAndRetryAsync(delay);



                // *****************************************************************
                logger.Debug("Attempting to fetch DeploymentInfo from MongoDB using Retry policy");

                var result = await policy.ExecuteAsync(async () =>
                {

                    var cursor = Collection.Find(b => b.Name == DeploymentName);

                    var info = await cursor.FirstOrDefaultAsync();
                    if (info == null)
                        throw new Exception($"Could not find Deployment using Name: ({DeploymentName})");

                    return info;

                });

                logger.LogObject(nameof(result), result);



                // *****************************************************************
                return result;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

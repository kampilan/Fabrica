using System;
using System.IO;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Autofac;
using Fabrica.Aws;
using Fabrica.Static.Monitors;
using Fabrica.Static.Providers.Mutable;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace Fabrica.Static.Appliance
{

    public enum DeploymentKind { Partial, Fixed, MongoDb, DynamoDb}

    public class TheModule: Module, IAwsCredentialModule, IFixedPackageMonitorModule, IMongoDbMonitorModule, IDynamoDbMonitorModule
    {


        public string Profile { get; set; } = "";
        public string RegionName  { get; set; } = "";

        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";

        public bool RunningOnEC2 { get; set; } = false;


        public string DeploymentType { get; set; } = "Partial";


        public bool EnableDirectoryBrowsing { get; set; } = false;
        public bool EnableDefaultFiles { get; set; } = true;


        public string MongoDbServerUri { get; set; } = "mongodb://localhost:27017";
        public string MongoDbDatabase { get; set; } = "fabrica_static";


        public string LocalInstallationPath { get; set; } = Path.GetTempPath();
        public string DeploymentName { get; set; } = "";
        public int DeploymentMonitorIntervalSecs { get; set; } = 10;


        public string FixedPackageRespository { get; set; } = "";
        public string FixedPackageLocation { get; set; } = "";


        public string TableNamePrefix { get; set; } = "";


        protected override void Load( ContainerBuilder builder )
        {

            builder.AddCorrelation();

            builder.UseAws(this);


            var type = (DeploymentKind)Enum.Parse(typeof(DeploymentKind), DeploymentType, true );
            switch( type )
            {

                case DeploymentKind.Partial:
                    RegisterPartialFileServerOptions(builder);
                    break;

                case DeploymentKind.MongoDb:
                    builder.UseAws(this);
                    RegisterMutableFileServerOptions(builder);
                    RegisterMongoDbPackageMonitor(builder);
                    break;

                case DeploymentKind.DynamoDb:
                    builder.UseAws(this);
                    RegisterMutableFileServerOptions(builder);
                    RegisterDynamoDbPackageMonitor(builder);
                    break;

                case DeploymentKind.Fixed:
                    builder.UseAws(this);
                    RegisterMutableFileServerOptions(builder);
                    RegisterFixedPackageMonitor(builder);
                    break;

                default:
                    throw new Exception( $"Invalid DeploymentType: {DeploymentType}" );

            }


        }


        protected void RegisterPartialFileServerOptions( ContainerBuilder builder )
        {

            builder.Register(_ =>
            {

                var comp = new FileServerOptions
                {
                    EnableDirectoryBrowsing = EnableDirectoryBrowsing,
                    EnableDefaultFiles      = EnableDefaultFiles,
                };

                comp.DefaultFilesOptions.DefaultFileNames.Clear();
                comp.DefaultFilesOptions.DefaultFileNames.Add("index.html");


                return comp;

            })
                .AsSelf()
                .SingleInstance();

        }

        protected void RegisterMutableFileServerOptions( ContainerBuilder builder )
        {

            Directory.CreateDirectory(LocalInstallationPath);


            builder.Register(_ =>
                {

                    var comp = new MutableDirectoryFileProvider( LocalInstallationPath );

                    return comp;

                })
                .AsSelf()
                .As<IFileProvider>()
                .SingleInstance();


            builder.Register(c =>
                {

                    var provider = c.Resolve<IFileProvider>();

                    var comp = new FileServerOptions
                    {
                        EnableDirectoryBrowsing = EnableDirectoryBrowsing,
                        EnableDefaultFiles      = EnableDefaultFiles,
                        FileProvider            = provider
                    };

                    comp.DefaultFilesOptions.DefaultFileNames.Clear();
                    comp.DefaultFilesOptions.DefaultFileNames.Add("index.html");


                    return comp;

                })
                .AsSelf()
                .SingleInstance();


        }

        protected void RegisterFixedPackageMonitor( ContainerBuilder builder )
        {


            builder.Register(c =>
                {

                    var client = c.Resolve<IAmazonS3>();
                    var provider = c.Resolve<MutableDirectoryFileProvider>();

                    var comp = new FixedPackageMonitor(this, client, provider );

                    return comp;

                })
                .AsSelf()
                .SingleInstance()
                .As<IStartable>()
                .AutoActivate();


        }

        protected void RegisterMongoDbPackageMonitor( ContainerBuilder builder )
        {


            builder.Register(c =>
            {

                var client   = c.Resolve<IAmazonS3>();
                var provider = c.Resolve<MutableDirectoryFileProvider>();

                var comp = new MongoDbPackageMonitor(this, client, provider);

                return comp;

            })
                .AsSelf()
                .SingleInstance()
                .As<IStartable>()
                .AutoActivate();

        }

        protected void RegisterDynamoDbPackageMonitor( ContainerBuilder builder )
        {


            builder.Register(c =>
                {

                    var dynaDb   = c.Resolve<IAmazonDynamoDB>();
                    var client   = c.Resolve<IAmazonS3>();
                    var provider = c.Resolve<MutableDirectoryFileProvider>();

                    var comp = new DynamoDbPackageMonitor(dynaDb, this, client, provider);

                    return comp;

                })
                .AsSelf()
                .SingleInstance()
                .As<IStartable>()
                .AutoActivate();

        }

    }


}

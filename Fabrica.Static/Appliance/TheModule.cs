using System;
using System.IO;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Api.Support.Filters;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Aws;
using Fabrica.Models.Serialization;
using Fabrica.Static.Monitors;
using Fabrica.Static.Providers.Mutable;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace Fabrica.Static.Appliance
{

    public enum DeploymentKind { Partial, Fixed, MongoDb, DynamoDb}

    public class TheModule: BootstrapModule, IAwsCredentialModule, IFixedPackageMonitorModule, IMongoDbMonitorModule, IDynamoDbMonitorModule
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

        public override void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc(o =>
                {
                    o.Filters.Add(typeof(ExceptionFilter));
                    o.Filters.Add(typeof(ResultFilter));
                })
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ContractResolver = new ModelContractResolver();
                    opt.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                });


        }


        public override void ConfigureContainer(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            builder.UseAws(this);


            var type = (DeploymentKind)Enum.Parse(typeof(DeploymentKind), DeploymentType, true);
            switch (type)
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
                    throw new Exception($"Invalid DeploymentType: {DeploymentType}");

            }


        }


        public override void ConfigureWebApp(IApplicationBuilder builder)
        {

            var scope = builder.ApplicationServices.GetAutofacRoot();

            builder.UsePipelineMonitor();
            builder.UseDebugMode();
            builder.UseRequestLogging();

            var options = scope.Resolve<FileServerOptions>();
            builder.UseFileServer(options);

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
                .As<IRequiresStart>()
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
                .As<IRequiresStart>()
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
                .As<IRequiresStart>()
                .AutoActivate();

        }


    }


}

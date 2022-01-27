using System.IO;
using Amazon.AppConfig;
using Amazon.S3;
using Autofac;
using Fabrica.Aws;
using Fabrica.Exceptions;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Orchestrator.Aws.Loader;
using Fabrica.One.Orchestrator.Aws.Plan;
using Fabrica.One.Plan;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.One.Orchestrator.Aws.Configuration
{


    public class OneOrchestratorModule: Module, IAwsCredentialModule
    {

        
        public string Profile { get; set; } = "";
        public string RegionName { get; set; } = "";
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public bool RunningOnEC2 { get; set; } = true;

        public string OneRoot { get; set; } = "";

        public string MissionPlanDir { get; set; } = "";
        public string MissionPlanName { get; set; } = "";
        public string RepositoryRoot { get; set; } = "";
        public string InstallationRoot { get; set; } = "";


        public bool UseInstanceMetadata { get; set; } = true;
        public string AppConfigPlanSourceApplication { get; set; } = "";
        public string AppConfigPlanSourceEnvironment { get; set; } = "";
        public string AppConfigPlanSourceConfiguration { get; set; } = "";

        public string RepositoryBucketName { get; set; } = "";

        protected override void Load(ContainerBuilder builder)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                logger.Inspect(nameof(OneRoot), OneRoot);
                if (string.IsNullOrWhiteSpace(OneRoot))
                    throw new InvalidConfigurationException(nameof(OneOrchestratorModule), nameof(OneRoot), "Missing required property");

                logger.Inspect(nameof(RepositoryBucketName), RepositoryBucketName);
                if (string.IsNullOrWhiteSpace(RepositoryBucketName))
                    throw new InvalidConfigurationException(nameof(OneOrchestratorModule), nameof(RepositoryBucketName), "Missing required property");

                logger.Inspect(nameof(AppConfigPlanSourceApplication), AppConfigPlanSourceApplication);
                if (string.IsNullOrWhiteSpace(AppConfigPlanSourceApplication))
                    throw new InvalidConfigurationException(nameof(OneOrchestratorModule), nameof(AppConfigPlanSourceApplication), "Missing required property");

                logger.Inspect(nameof(AppConfigPlanSourceEnvironment), AppConfigPlanSourceEnvironment);
                if (string.IsNullOrWhiteSpace(AppConfigPlanSourceEnvironment))
                    throw new InvalidConfigurationException(nameof(OneOrchestratorModule), nameof(AppConfigPlanSourceEnvironment), "Missing required property");

                logger.Inspect(nameof(AppConfigPlanSourceConfiguration), AppConfigPlanSourceConfiguration);
                if (string.IsNullOrWhiteSpace(AppConfigPlanSourceConfiguration))
                    throw new InvalidConfigurationException(nameof(OneOrchestratorModule), nameof(AppConfigPlanSourceConfiguration), "Missing required property");


                logger.Inspect(nameof(MissionPlanDir), MissionPlanDir);
                if (string.IsNullOrWhiteSpace(MissionPlanDir))
                    MissionPlanDir = $"{OneRoot}";

                logger.Inspect(nameof(MissionPlanName), MissionPlanName);
                if (string.IsNullOrWhiteSpace(MissionPlanName))
                    MissionPlanName = "mission-plan.json";

                logger.Inspect(nameof(RepositoryRoot), RepositoryRoot);
                if (string.IsNullOrWhiteSpace(RepositoryRoot))
                    RepositoryRoot = $"{OneRoot}{Path.DirectorySeparatorChar}repository";

                logger.Inspect(nameof(InstallationRoot), InstallationRoot);
                if (string.IsNullOrWhiteSpace(InstallationRoot))
                    InstallationRoot = $"{OneRoot}{Path.DirectorySeparatorChar}installations";



                builder.UseAws(this);


                // *****************************************************************
                logger.Debug("Attempting to register AppConfigPlanSource");
                builder.Register(c =>
                    {

                        var client = c.Resolve<IAmazonAppConfig>();

                        var comp = new AppConfigPlanSource(client)
                        {
                            UseInstanceMetadata  = UseInstanceMetadata,
                            Application          = AppConfigPlanSourceApplication,
                            Environment          = AppConfigPlanSourceEnvironment,
                            Configuration        = AppConfigPlanSourceConfiguration
                        };

                        return comp;

                    })
                    .As<IPlanSource>()
                    .As<IRequiresStart>()
                    .AutoActivate()
                    .SingleInstance();



                // *****************************************************************
                logger.Debug("Attempting to register MissionOchestrator");
                builder.Register(c =>
                    {

                        var factory   = c.Resolve<IPlanFactory>();
                        var source    = c.Resolve<IPlanSource>();
                        var writer    = c.Resolve<IPlanWriter>();
                        var loader    = c.Resolve<IApplianceLoader>();
                        var installer = c.Resolve<IApplianceInstaller>();

                        var comp = new MissionOrchestrator(factory, source, writer, loader, installer);

                        return comp;

                    })
                    .AsSelf()
                    .SingleInstance();



                // *****************************************************************
                logger.Debug("Attempting to build PlanFactory");
                builder.Register(c =>
                    {

                        var factory = new JsonPlanFactory(RepositoryRoot, InstallationRoot);
                        return factory;

                    })
                    .As<IPlanFactory>()
                    .InstancePerDependency();



                // *****************************************************************
                logger.Debug("Attempting to register S3 appliance loader");
                builder.Register(c =>
                    {

                        var client = c.Resolve<IAmazonS3>();

                        var comp = new S3ApplianceLoader(client, RepositoryBucketName );

                        return comp;

                    })
                    .As<IApplianceLoader>()
                    .InstancePerDependency();



                // *****************************************************************
                logger.Debug("Attempting to register Repository appliance installer");
                builder.Register(c =>
                    {

                        var installer = new FileRepositoryInstaller();

                        return installer;

                    })
                    .As<IApplianceInstaller>()
                    .InstancePerDependency();



                // *****************************************************************
                logger.Debug("Attempting to register Plan writer");
                builder.Register(c =>
                    {


                        var comp = new FilePlanWriter
                        {
                            MissionFileDir  = MissionPlanDir,
                            MissionFileName = MissionPlanName
                        };

                        return comp;


                    })
                    .As<IPlanWriter>()
                    .InstancePerDependency();



            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

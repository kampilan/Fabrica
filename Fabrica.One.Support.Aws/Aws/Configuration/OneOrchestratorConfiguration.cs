﻿using Amazon;
using Amazon.AppConfigData;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Fabrica.Aws;
using Fabrica.Exceptions;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Plan;
using Fabrica.One.Support.Aws.Loader;
using Fabrica.One.Support.Aws.Plan;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.One.Support.Aws.Configuration;

public class OneOrchestratorConfiguration: Module
{
        
    public string AwsProfileName { get; set; } = "";
    public string AwsRegionName { get; set; } = "";

    public string OneRoot { get; set; } = "";

    public string MissionPlanDir { get; set; } = "";
    public string MissionPlanName { get; set; } = "";
    public string RepositoryRoot { get; set; } = "";
    public string InstallationRoot { get; set; } = "";


    public bool UseInstanceMetadata { get; set; } = true;
    public int AppConfigCheckIntervalSecs { get; set; } = 30;
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
                throw new InvalidConfigurationException(nameof(OneOrchestratorConfiguration), nameof(OneRoot), "Missing required property");

            logger.Inspect(nameof(RepositoryBucketName), RepositoryBucketName);
            if (string.IsNullOrWhiteSpace(RepositoryBucketName))
                throw new InvalidConfigurationException(nameof(OneOrchestratorConfiguration), nameof(RepositoryBucketName), "Missing required property");

            logger.Inspect(nameof(AppConfigPlanSourceApplication), AppConfigPlanSourceApplication);
            if ( !UseInstanceMetadata && string.IsNullOrWhiteSpace(AppConfigPlanSourceApplication))
                throw new InvalidConfigurationException(nameof(OneOrchestratorConfiguration), nameof(AppConfigPlanSourceApplication), "Missing required property");

            logger.Inspect(nameof(AppConfigPlanSourceEnvironment), AppConfigPlanSourceEnvironment);
            if (!UseInstanceMetadata && string.IsNullOrWhiteSpace(AppConfigPlanSourceEnvironment))
                throw new InvalidConfigurationException(nameof(OneOrchestratorConfiguration), nameof(AppConfigPlanSourceEnvironment), "Missing required property");

            logger.Inspect(nameof(AppConfigPlanSourceConfiguration), AppConfigPlanSourceConfiguration);
            if (!UseInstanceMetadata && string.IsNullOrWhiteSpace(AppConfigPlanSourceConfiguration))
                throw new InvalidConfigurationException(nameof(OneOrchestratorConfiguration), nameof(AppConfigPlanSourceConfiguration), "Missing required property");


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



            builder.UseAws(AwsProfileName)
                .AddS3Client(AwsRegionName);


            builder.Register(c =>
                {

                    RegionEndpoint? region = null;
                    if (!string.IsNullOrWhiteSpace(AwsRegionName))
                        region = RegionEndpoint.GetBySystemName(AwsRegionName);

                    var credentials = c.ResolveOptional<AWSCredentials>();

                    if (credentials is not null && region is not null)
                        return new AmazonAppConfigDataClient(credentials, region);

                    if (region is not null)
                        return new AmazonAppConfigDataClient(region);

                    if( credentials is not null)
                        return new AmazonAppConfigDataClient(credentials);


                    return new AmazonAppConfigDataClient();


                })
                .As<IAmazonAppConfigData>()
                .SingleInstance();



            // *****************************************************************
            logger.Debug("Attempting to register AppConfigPlanSource");
            builder.Register(c =>
                {

                    var ts = TimeSpan.FromSeconds(AppConfigCheckIntervalSecs);
                    if( ts.TotalSeconds < 15 )
                        ts = TimeSpan.FromSeconds(15);

                    var client = c.Resolve<IAmazonAppConfigData>();

                    var comp = new AppConfigPlanSource(client)
                    {
                        UseInstanceMetadata  = UseInstanceMetadata,
                        Application          = AppConfigPlanSourceApplication,
                        Environment          = AppConfigPlanSourceEnvironment,
                        Configuration        = AppConfigPlanSourceConfiguration,
                        CheckInterval        = ts
                    };

                    return comp;

                })
                .As<IPlanSource>()
                .As<IRequiresStart>()
                .AutoActivate()
                .SingleInstance();



            // *****************************************************************
            logger.Debug("Attempting to register MissionOrchestrator");
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
            builder.Register( _ =>
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
            builder.Register( _ =>
                {

                    var installer = new FileRepositoryInstaller();

                    return installer;

                })
                .As<IApplianceInstaller>()
                .InstancePerDependency();



            // *****************************************************************
            logger.Debug("Attempting to register Plan writer");
            builder.Register( _ =>
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
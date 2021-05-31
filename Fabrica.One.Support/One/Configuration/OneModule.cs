using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Amazon.AppConfig;
using Amazon.S3;
using Amazon.Util;
using Autofac;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Plan;
using Fabrica.Utilities.Threading;
using Fabrica.Watch;

namespace Fabrica.One.Configuration
{


    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class OneModule: Module
    {


        public string InstallationRoot { get; set; } = "";


        public string FileSysRepositoryPath { get; set; } = "";

        
        public bool UseAppConfigPlanSource { get; set; } = false;
        public string AppConfigPlanSourceApplication { get; set; } = "";
        public string AppConfigPlanSourceEnvironment { get; set; } = "production";
        public string AppConfigPlanSourceConfiguration { get; set; } = "mission";


        public bool UseInstanceMetaData { get; set; } = false;
        public bool RunningOnEC2 { get; set; } = true;


        public bool ExternalPlanSource { get; set; } = false;
        public string FileSysPlanSourcePath { get; set; } = "local.mission.yml";


        protected override void Load( ContainerBuilder builder )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                if( !string.IsNullOrWhiteSpace(FileSysRepositoryPath) )
                {

                    builder.Register(c =>
                        {

                            var comp = new FileSysApplianceLoader
                            {
                                LocalPath = FileSysRepositoryPath
                            };

                            return comp;

                        })
                        .As<IApplianceLoader>()
                        .InstancePerDependency();

                }
                else
                {

                    builder.Register(c =>
                        {

                            var client = c.Resolve<IAmazonS3>();

                            var comp = new S3ApplianceLoader( client );

                            return comp;

                        })
                        .As<IApplianceLoader>()
                        .InstancePerDependency();

                }


                if( UseAppConfigPlanSource )
                {

                    builder.Register(c =>
                        {

                            var client = c.Resolve<IAmazonAppConfig>();

                            var comp = new AppConfigPlanSource( client )
                            {
                                Application   = AppConfigPlanSourceApplication,
                                Environment   = AppConfigPlanSourceEnvironment,
                                Configuration = AppConfigPlanSourceConfiguration,
                                RunningOnEC2  = RunningOnEC2
                            };

                            return comp;

                        })
                        .As<IPlanSource>()
                        .As<IStartable>()
                        .AutoActivate()
                        .SingleInstance();



                }
                else if( UseInstanceMetaData && RunningOnEC2 )
                {

                    builder.Register(c =>
                        {

                            var comp = new MemoryPlanSource();

                            using (var strm = new MemoryStream())
                            using (var writer = new StreamWriter(strm))
                            {

                                writer.Write(EC2InstanceMetadata.UserData);
                                writer.Flush();

                                strm.Seek(0, SeekOrigin.Begin);

                                comp.CopyFrom(strm);

                            }


                            return comp;

                        })
                        .As<IPlanSource>()
                        .AutoActivate()
                        .SingleInstance();

                }
                else if( !ExternalPlanSource )
                {

                    builder.Register(c =>
                        {

                            var comp = new FilePlanSource
                            {
                                FilePath = FileSysPlanSourcePath
                            };

                            return comp;

                        })
                        .As<IPlanSource>()
                        .AutoActivate()
                        .SingleInstance();

                }


                // *****************************************************************
                logger.Debug("Attempting to build PlanFactory");
                builder.Register(c =>
                    {

                        var factory = new YamlPlanFactory();

                        if( !string.IsNullOrWhiteSpace(InstallationRoot) )
                            factory.InstallationRoot = InstallationRoot;

                        return factory;

                    })
                    .As<IPlanFactory>()
                    .InstancePerDependency();



                // *****************************************************************
                logger.Debug("Attempting to build Installer");
                builder.Register(c =>
                    {

                        var installer = new ZipInstaller();
                        return installer;

                    })
                    .As<IApplianceInstaller>()
                    .InstancePerDependency();



                // *****************************************************************
                logger.Debug("Attempting to build Appliance Factory");
                builder.Register(c =>
                    {

                        var factory = new ApplianceFactory();
                        return factory;

                    })
                    .As<IApplianceFactory>()
                    .InstancePerDependency();



                builder.Register(c =>
                    {

                        var plan         = c.Resolve<IPlan>();
                        var appLoader    = c.Resolve<IApplianceLoader>();
                        var appInstaller = c.Resolve<IApplianceInstaller>();
                        var appFactory   = c.Resolve<IApplianceFactory>();

                        var comp = new Mission( plan, appLoader, appInstaller, appFactory );

                        return comp;

                    })
                    .AsSelf()
                    .InstancePerDependency();



                builder.Register( c =>
                    {

                        var source  = c.Resolve<IPlanSource>();
                        var factory = c.Resolve<IPlanFactory>();

                        var stream = AsyncPump.Run( async()=> await source.GetSource());

                        var plan = factory.Create( stream );

                        return plan;

                    })
                    .As<IPlan>()
                    .InstancePerDependency();


                builder.Register(c =>
                    {

                        var scope = c.Resolve<ILifetimeScope>();

                        var comp = new MissionObserver(scope);

                        return comp;

                    })
                    .AsSelf()
                    .SingleInstance()
                    .AutoActivate();


            }
            catch ( Exception cause )
            {
                logger.Error( cause, "Exception caught during One Deployment Module Load");
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }

        }



    }


}

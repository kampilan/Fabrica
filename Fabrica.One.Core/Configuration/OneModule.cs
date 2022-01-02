using System;
using Autofac;
using Fabrica.Exceptions;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Plan;
using Fabrica.Utilities.Threading;
using Fabrica.Watch;

namespace Fabrica.One.Configuration
{


    public class OneModule: Module
    {

        public string RepositoryRoot { get; set; } = "";
        public string InstallationRoot { get; set; } = "";

        public bool ExternalPlanSource { get; set; } = true;
        public string FileSystemPlanDir { get; set; } = "./";
        public string FileSystemPlanName { get; set; } = "mission-plan.json";

        protected override void Load( ContainerBuilder builder )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(RepositoryRoot), RepositoryRoot);
                if (string.IsNullOrWhiteSpace(RepositoryRoot))
                    throw new InvalidConfigurationException(nameof(OneModule), nameof(RepositoryRoot), "Missing required property" );

                logger.Inspect(nameof(InstallationRoot), InstallationRoot);
                if (string.IsNullOrWhiteSpace(InstallationRoot))
                    throw new InvalidConfigurationException(nameof(OneModule), nameof(InstallationRoot), "Missing required property" );



                // *****************************************************************
                logger.Debug("Attempting to MemoryPlanSource");
                builder.Register(c =>
                    {
                        var comp = new MemoryPlanSource();
                        return comp;

                    })
                    .AsSelf()
                    .As<IPlanSource>()
                    .AutoActivate()
                    .SingleInstance();



                // *****************************************************************
                logger.Inspect(nameof(ExternalPlanSource), ExternalPlanSource);
                if( !ExternalPlanSource )
                {

                    logger.Inspect(nameof(FileSystemPlanDir), FileSystemPlanDir);
                    if (string.IsNullOrWhiteSpace(FileSystemPlanDir))
                        throw new InvalidConfigurationException(nameof(OneModule), nameof(FileSystemPlanDir), "Missing required property");

                    logger.Inspect(nameof(FileSystemPlanName), FileSystemPlanName);
                    if (string.IsNullOrWhiteSpace(FileSystemPlanName))
                        throw new InvalidConfigurationException(nameof(OneModule), nameof(FileSystemPlanName), "Missing required property");


                    builder.Register(c =>
                        {

                            var comp = new FilePlanSource
                            {
                                FileDir  = FileSystemPlanDir,
                                FileName = FileSystemPlanName
                            };

                            return comp;

                        })
                        .AsSelf()
                        .As<IPlanSource>()
                        .AutoActivate()
                        .SingleInstance();

                }



                // *****************************************************************
                logger.Debug("Attempting to register MissionObserver");
                builder.Register(c =>
                    {

                        var scope = c.Resolve<ILifetimeScope>();

                        var comp = new MissionObserver(scope);

                        return comp;

                    })
                    .AsSelf()
                    .AutoActivate()
                    .SingleInstance();



                // *****************************************************************
                logger.Debug("Attempting to build PlanFactory");
                builder.Register(c =>
                    {

                        var factory = new JsonPlanFactory( RepositoryRoot, InstallationRoot );
                        return factory;

                    })
                    .As<IPlanFactory>()
                    .InstancePerLifetimeScope();



                // *****************************************************************
                logger.Debug("Attempting to register FileSys Loader");
                builder.Register(c =>
                    {
                        var comp = new FileSysApplianceLoader();
                        return comp;
                    })
                    .As<IApplianceLoader>()
                    .InstancePerLifetimeScope();


                // *****************************************************************
                logger.Debug("Attempting to build Installer");
                builder.Register(c =>
                    {

                        var installer = new ZipInstaller();

                        return installer;

                    })
                    .As<IApplianceInstaller>()
                    .InstancePerLifetimeScope();



                // *****************************************************************
                logger.Debug("Attempting to build Appliance Factory");
                builder.Register(c =>
                    {

                        var factory = new ApplianceFactory();
                        return factory;

                    })
                    .As<IApplianceFactory>()
                    .InstancePerLifetimeScope();



                builder.Register(c =>
                    {

                        var plan      = c.Resolve<IPlan>();
                        var loader    = c.Resolve<IApplianceLoader>();
                        var installer = c.Resolve<IApplianceInstaller>();
                        var factory   = c.Resolve<IApplianceFactory>();

                        var comp = new Mission(plan, loader, installer, factory);

                        return comp;

                    })
                    .AsSelf()
                    .InstancePerLifetimeScope();



                builder.Register(c =>
                    {

                        var source  = c.Resolve<IPlanSource>();
                        var factory = c.Resolve<IPlanFactory>();

                        var stream = AsyncPump.Run(async () => await source.GetSource());

                        var plan = factory.Create(stream);

                        return plan;

                    })
                    .As<IPlan>()
                    .InstancePerLifetimeScope();



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

using System;
using System.IO;
using Autofac;
using Fabrica.Exceptions;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Plan;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.One.Configuration
{


    public class OneMissionModule: Module
    {


        public string OneRoot { get; set; } = "";

        public string MissionPlanDir { get; set; } = "";
        public string MissionPlanName { get; set; } = "";
        public string RepositoryRoot { get; set; } = "";
        public string InstallationRoot { get; set; } = "";

        public bool UseExternalPlanSource { get; set; } = true;

        public bool ProduceEmptyPlans { get; set; } = true;

        public bool UnderOrchestration { get; set; } = true;

        protected override void Load( ContainerBuilder builder )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(OneRoot), OneRoot);
                if( string.IsNullOrWhiteSpace(OneRoot) )
                    throw new InvalidConfigurationException(nameof(OneMissionModule), nameof(OneRoot), "Missing required property");

                logger.Inspect(nameof(MissionPlanDir), MissionPlanDir);
                if (string.IsNullOrWhiteSpace(MissionPlanDir))
                    MissionPlanDir = $"{OneRoot}";

                logger.Inspect(nameof(MissionPlanName), MissionPlanName);
                if (string.IsNullOrWhiteSpace(MissionPlanName))
                    MissionPlanName = "mission-plan.json";

                logger.Inspect(nameof(RepositoryRoot), RepositoryRoot);
                if( string.IsNullOrWhiteSpace(RepositoryRoot) )
                    RepositoryRoot = $"{OneRoot}{Path.DirectorySeparatorChar}repository";

                logger.Inspect(nameof(InstallationRoot), InstallationRoot);
                if( string.IsNullOrWhiteSpace(InstallationRoot) )
                    InstallationRoot = $"{OneRoot}{Path.DirectorySeparatorChar}installations";



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
                logger.Inspect(nameof(UseExternalPlanSource), UseExternalPlanSource);
                if( UseExternalPlanSource )
                {


                    builder.Register(c =>
                        {

                            var comp = new FilePlanSource
                            {
                                FileDir  = MissionPlanDir,
                                FileName = MissionPlanName
                            };

                            return comp;

                        })
                        .AsSelf()
                        .As<IPlanSource>()
                        .As<IRequiresStart>()
                        .AutoActivate()
                        .SingleInstance();

                }



                // *****************************************************************
                logger.Debug("Attempting to register MissionObserver");
                builder.Register(c =>
                    {

                        var scope = c.Resolve<ILifetimeScope>();

                        var comp = new MissionObserver(scope)
                        {
                            MissionStatusDir = MissionPlanDir
                        };

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

                        var plan = factory.Create(source, ProduceEmptyPlans).ConfigureAwait(false).GetAwaiter().GetResult();

                        return plan;

                    })
                    .As<IPlan>()
                    .InstancePerLifetimeScope();



            }
            catch ( Exception cause )
            {
                logger.Error( cause, "Exception caught during OneMissionModule Load");
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }

        }



    }


}

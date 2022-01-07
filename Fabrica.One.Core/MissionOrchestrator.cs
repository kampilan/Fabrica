using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Plan;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.One
{


    public class MissionOrchestrator
    {


        public MissionOrchestrator([NotNull] IPlanFactory factory, [NotNull] IPlanSource source, IPlanWriter writer, [NotNull] IApplianceLoader loader, [NotNull] IApplianceInstaller installer)
        {

            Factory   = factory;
            Source    = source;
            Writer    = writer;
            Loader    = loader;
            Installer = installer;

        }


        public TimeSpan UpdateCheckPollingInterval { get; set; } = TimeSpan.FromSeconds(5);


        private IPlanSource Source { get; }
        private IPlanFactory Factory { get; }
        private IPlanWriter Writer { get; }
        private IApplianceLoader Loader { get; }
        private IApplianceInstaller Installer { get; }


        private DateTime LastCheck { get; set; } = DateTime.Now.AddHours(-24);
        public async Task CheckForUpdatedPlan()
        {


            if( LastCheck + UpdateCheckPollingInterval >= DateTime.Now )
                return;


            var updated = await Source.HasUpdatedPlan();
            if (updated)
                await ProcessNewPlan();

        }


        protected virtual async Task ProcessNewPlan()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to create mission plan");
                var plan = await Factory.Create(Source, true);



                // *****************************************************************
                logger.Debug("Attempting to configure plan");
                await ConfigurePlan(plan);



                // *****************************************************************
                logger.Debug("Attempting to clean loader");
                await Loader.Clean(plan);



                // *****************************************************************
                logger.Debug("Attempting to clean loader");
                await Installer.Clean(plan);



                // *****************************************************************
                logger.Debug("Attempting to load each deployment unit");
                var set = new HashSet<string>();

                foreach (var unit in plan.Deployments)
                {


                    // *****************************************************************
                    logger.Debug("Attempting to check for duplicate deployments");
                    var id = $"{unit.Name}-{unit.Build}";
                    logger.Inspect(nameof(id), id);

                    if (set.Contains(id))
                    {
                        logger.Debug("This appliance was already installed");
                        continue;
                    }

                    set.Add(id);



                    // *****************************************************************
                    logger.Debug("Attempting to load unit");
                    await Loader.Load(plan, unit);



                    // *****************************************************************
                    logger.Debug("Attempting to install unit");
                    await Installer.Install(plan, unit);


                }



                // *****************************************************************
                logger.Debug("Attempting to save updated plan");
                await Factory.Save(plan, Writer);



                // *****************************************************************
                logger.Debug("Attempting to update LastCheck");
                LastCheck = DateTime.Now;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected virtual async Task ConfigurePlan(IPlan plan)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to create and set new RepositoryVersion");
                var repoVersion = DateTime.UtcNow.ToTimestampString();
                logger.Inspect(nameof(repoVersion), repoVersion);

                plan.SetRepositoryVersion(repoVersion);

                await Factory.CreateRepositoryVersion(plan);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

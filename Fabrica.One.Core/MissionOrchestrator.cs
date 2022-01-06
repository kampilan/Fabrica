using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Exceptions;
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

        public MissionOrchestrator([NotNull] IPlanFactory factory, [NotNull] IPlanSource source, IPlanWriter writer, [NotNull] IApplianceLoader loader, [NotNull] IApplianceInstaller installer )
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


        private CancellationTokenSource MustStopSource { get; } = new CancellationTokenSource();
        private CancellationToken MustStopToken { get; set; } = new CancellationToken(true);
        private Task RunTask { get; set; }


        private Result RunResult { get; } = new Result();


        public async Task<bool> RunOnce()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to check for an update");
                var updated = await _check();

                logger.Inspect(nameof(updated), updated);


                // *****************************************************************
                return updated;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public Result Run()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                RunResult.Successful = false;
                RunResult.Details.Clear();



                // *****************************************************************
                logger.Debug("Attempting to check if orchestrator is already running");
                if (RunTask != null && !RunTask.IsCompleted)
                {
                    logger.Debug("MissionOrchestrator is already running.");
                    RunResult.Successful = true;
                    RunResult.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Run", Explanation = "Orchestator is already running", Source = "Orchestator" });
                    return RunResult;
                }



                // *****************************************************************
                logger.Debug("Attempting to start background task");
                MustStopToken = MustStopSource.Token;
                RunTask = Task.Run(() => _run());


                RunResult.Successful = true;


                // *****************************************************************
                return RunResult;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public Result Terminate()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                var result = new Result { Successful = true };


                if (RunTask == null)
                {
                    logger.Debug("Orchestator is not running");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Orchestator is not running", Source = "Orchestator" });
                    return result;

                }

                if (RunTask.IsCompleted)
                {
                    logger.Debug("Orchestator has already been stopped");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Orchestator has already been stopped", Source = "Orchestator" });
                    return result;
                }

                if (!RunTask.IsCompleted && MustStopToken.IsCancellationRequested)
                {
                    logger.Debug("Orchestator in the process of being stopped");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Orchestator in the process of being stopped", Source = "Orchestator" });
                    return result;
                }



                // *****************************************************************
                logger.Debug("Attempting to signal Mission to stop");
                MustStopSource.Cancel();



                // *****************************************************************
                logger.Debug("Attempting to wait for Mission to stop");
                result.Successful = RunTask.Wait(TimeSpan.FromSeconds(10 + 10));

                RunTask = null;


                // *****************************************************************
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected virtual async Task ConfigurePlan( IPlan plan )
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


        private async Task _run()
        {


            // *****************************************************************
            while( !MustStopToken.IsCancellationRequested )
            {

                await _check();

                await Task.Delay( UpdateCheckPollingInterval, MustStopToken );

            }


        }


        private async Task<bool> _check()
        {

            var updated = await Source.HasUpdatedPlan();
            if (updated)
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

                    var set = new HashSet<string>();

                    // *****************************************************************
                    logger.Debug("Attempting to load each deployment unit");
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



                }
                finally
                {
                    logger.LeaveMethod();
                }


            }

            return updated;

        }


    }


}

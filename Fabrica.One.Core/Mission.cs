using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Models;
using Fabrica.One.Plan;
using Fabrica.Watch;
using Fabrica.Watch.Sink;
using JetBrains.Annotations;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Fabrica.One
{
    public class Mission
    {




        public Mission([NotNull] IPlan plan, [NotNull] IApplianceLoader appLoader, [NotNull] IApplianceInstaller appInstaller, [NotNull] IApplianceFactory appFactory)
        {

            Plan = plan;

            ApplianceLoader = appLoader;
            ApplianceInstaller = appInstaller;
            ApplianceFactory = appFactory;

        }

        public bool UnderOrchestration { get; set; } = true;

        private IPlan Plan { get; }

        private IApplianceLoader ApplianceLoader { get; }
        private IApplianceInstaller ApplianceInstaller { get; }
        private IApplianceFactory ApplianceFactory { get; }


        public void Reset()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                RunResult.Successful = false;
                RunResult.Details.Clear();

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        public Result Clean()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var result = new Result { Successful = true };


                // *****************************************************************
                logger.Debug("Attempting to check if mission is already running");
                if (Appliances.Count > 0 && Appliances.Any(a => !a.HasStarted))
                {
                    logger.Debug("Mission is already running.");
                    result.Successful = false;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Violation, Group = "Clean", Explanation = "Mission is currently running", Source = "Mission" });

                    RunResult.Successful = result.Successful;
                    RunResult.Details.AddRange(result.Details);

                    return result;

                }



                // *****************************************************************
                logger.Debug("Attempting to clean Mission");
                _clean(Plan, RunResult);



                // *****************************************************************
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public Result Deploy()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var result = new Result { Successful = true };


                // *****************************************************************
                logger.Debug("Attempting to check if mission is already running");
                if (RunTask != null && !RunTask.IsCompleted)
                {

                    logger.Debug("Mission is already running.");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Deployment", Explanation = "Mission is already running", Source = "Mission" });

                    RunResult.Successful = result.Successful;
                    RunResult.Details.AddRange(result.Details);

                    return result;

                }



                // *****************************************************************
                logger.Debug("Attempting to deploy Mission");
                _deploy(Plan, RunResult);



                // *****************************************************************
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public Result Start()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var result = new Result { Successful = true };


                // *****************************************************************
                logger.Debug("Attempting to check if mission is already running");
                if (RunTask != null && !RunTask.IsCompleted)
                {

                    logger.Debug("Mission is already running.");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Start", Explanation = "Mission is already running", Source = "Mission" });

                    RunResult.Successful = result.Successful;
                    RunResult.Details.AddRange(result.Details);

                    return result;

                }



                // *****************************************************************
                logger.Debug("Attempting to start Mission");
                logger.LogObject(nameof(Plan), Plan);
                _start(Plan, RunResult);



                // *****************************************************************
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public Result Stop()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var result = new Result { Successful = true };


                if (Appliances.Count == 0 && StartComplete)
                {

                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Mission was empty", Source = "Mission" });

                    RunResult.Successful = result.Successful;
                    RunResult.Details.AddRange(result.Details);


                    return result;

                }


                if (Appliances.Count == 0)
                {

                    logger.Debug("Mission is not running");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Mission is not running", Source = "Mission" });

                    RunResult.Successful = result.Successful;
                    RunResult.Details.AddRange(result.Details);

                    return result;

                }


                if (Appliances.All(a => a.HasStopped))
                {

                    logger.Debug("Mission has already been stopped");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Mission has already been stopped", Source = "Mission" });

                    RunResult.Successful = result.Successful;
                    RunResult.Details.AddRange(result.Details);

                    return result;

                }



                // *****************************************************************
                logger.Debug("Attempting to stop Mission");
                logger.LogObject(nameof(Plan), Plan);
                _stop(Plan, result);



                // *****************************************************************
                return result;


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
                logger.Debug("Attempting to check if mission is already running");
                if (RunTask != null && !RunTask.IsCompleted)
                {
                    logger.Debug("Mission is already running.");
                    RunResult.Successful = true;
                    RunResult.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Run", Explanation = "Mission is already running", Source = "Mission" });
                    return RunResult;
                }



                // *****************************************************************
                logger.Debug("Attempting to start background task");
                logger.LogObject(nameof(Plan), Plan);
                MustStopToken = MustStopSource.Token;
                RunTask = Task.Run(() => _run(Plan));


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
                    logger.Debug("Mission is not running");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Mission is not running", Source = "Mission" });
                    return result;

                }

                if (RunTask.IsCompleted)
                {
                    logger.Debug("Mission has already been stopped");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Mission has already been stopped", Source = "Mission" });
                    return result;
                }

                if (!RunTask.IsCompleted && MustStopToken.IsCancellationRequested)
                {
                    logger.Debug("Mission in the process of being stopped");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Mission in the process of being stopped", Source = "Mission" });
                    return result;
                }



                // *****************************************************************
                logger.Debug("Attempting to signal Mission to stop");
                MustStopSource.Cancel();



                // *****************************************************************
                logger.Debug("Attempting to wait for Mission to stop");
                result.Successful = RunTask.Wait(TimeSpan.FromSeconds(Plan.WaitForStopSeconds + 10));

                RunTask = null;


                // *****************************************************************
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public bool StartComplete => Appliances.Count == 0 || Appliances.All(a => a.HasStarted);

        public IEnumerable<StatusModel> GetAppliances()
        {

            return Appliances.Select(ap => new StatusModel
            {
                Uid = ap.Unit.Uid,
                Alias = ap.Unit.Alias,
                Name = ap.Unit.Name,
                Build = ap.Unit.Build,
                RepositoryLocation = ap.Unit.RepositoryLocation,
                InstallationLocation = ap.Unit.InstallationLocation,
                Assembly = ap.Unit.Assembly,
                EnvironmentConfiguration = ap.Unit.Configuration,
                HasLoaded = ap.Unit.HasLoaded,
                HasInstalled = ap.Unit.HasInstalled,
                HasStarted = ap.HasStarted,
                HasStopped = ap.HasStopped
            })
                .ToList();

        }

        public string GetStatusAsJson()
        {

            var apps = GetAppliances();

            var status = new { Results = RunResult, Appliances = apps.ToList() };

            var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(status, options);


            return json;


        }

        private CancellationTokenSource MustStopSource { get; set; } = new CancellationTokenSource();
        private CancellationToken MustStopToken { get; set; } = new CancellationToken(true);
        private Task RunTask { get; set; }

        private Result RunResult { get; set; } = new Result();
        private IList<IAppliance> Appliances { get; } = new List<IAppliance>();


        private void _run(IPlan plan)
        {

            var logger = this.GetLogger();

            if (logger.IsInfoEnabled)
            {

                var builder = new StringBuilder();
                foreach (var u in plan.Deployments)
                    builder.AppendLine($"Appliance Alias: {u.Alias} Name: {u.Name} Build: {u.Build}");

                var ev = logger.CreateEvent(Level.Info, "Appliance Info", PayloadType.Yaml, builder.ToString());
                logger.LogEvent(ev);

            }


            var started = false;
            var sw = new Stopwatch();
            sw.Start();



            // *****************************************************************
            _clean(plan, RunResult);

            logger.InfoFormat("Clean completed in {0} msec(s)", sw.ElapsedMilliseconds);



            // *****************************************************************
            _deploy(plan, RunResult);

            logger.InfoFormat("Deployment completed in {0} msec(s)", sw.ElapsedMilliseconds);



            // *****************************************************************
            _start(plan, RunResult);

            logger.InfoFormat("Start completed in {0} msec(s)", sw.ElapsedMilliseconds);



            // *****************************************************************
            logger.Debug("Attempting to enter monitor loop");
            while (!MustStopToken.IsCancellationRequested)
            {

                if (!started && Appliances.Count == 0)
                {
                    sw.Stop();
                    logger.InfoFormat("Empty Mission deployed and started in {0} msec(s)", sw.ElapsedMilliseconds);
                    started = true;
                }

                if (!started && Appliances.All(a => !a.HasStarted))
                {
                    sw.Stop();
                    logger.InfoFormat("{0} appliances deployed and started in {1} msec(s)", Appliances.Count, sw.ElapsedMilliseconds);
                    started = true;
                }


                foreach (var mgr in Appliances.Where(a => a.HasStarted && a.HasStopped))
                {
                    this.GetLogger().ErrorFormat("Appliance: {0} has stopped unexpectedly, Restarting.", mgr.Unit.Alias);
                    Thread.Sleep(500);
                    mgr.Start();
                }

                Thread.Sleep(100);

            }

            this.GetLogger().Info("Stopping Appliances");

            _stop(plan, RunResult);


        }


        private void _clean(IPlan plan, Result result)
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to check if clean required");
                if (!Plan.DeployAppliances)
                {
                    logger.Debug("Skipping Clean per Plan.DeployAppliances=false");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Clean", Explanation = "Skipping Clean per Plan.DeployAppliances=false", Source = "Mission" });
                    return;
                }


                logger.Debug("Attempting to cleanup the repository");
                ApplianceLoader.Clean(plan);


                logger.Debug("Attempting to cleanup installations");
                ApplianceInstaller.Clean(plan);


                result.Successful = true;
                result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Clean", Explanation = "Mission clean performed", Source = "Mission" });


            }
            catch (AggregateException cause)
            {

                logger.Error(cause, "Mission clean failed");

                result.Successful = false;
                foreach (var ex in cause.InnerExceptions)
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Clean", Explanation = ex.Message, Source = "Mission" });
            }
            catch (Exception cause)
            {

                logger.Error(cause, "Mission clean failed");

                result.Successful = false;
                result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Clean", Explanation = cause.Message, Source = "Mission" });
            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        private void _deploy(IPlan plan, Result result)
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to check if deployment required");
                if (!Plan.DeployAppliances)
                {
                    logger.Debug("Skipping Deploy per Plan.DeployAppliances=false");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Deployment", Explanation = "Skipping Deploy per Plan.DeployAppliances=false", Source = "Mission" });
                    return;
                }



                logger.Debug("Attempting to deploy each appliance in parallel");
                var tasks = new List<Task>();
                foreach (var unit in plan.Deployments)
                {

                    var task = Task.Run(async () =>
                    {
                        await ApplianceLoader.Load(plan, unit);
                        await ApplianceInstaller.Install(plan, unit);
                    });


                    tasks.Add(task);

                }


                bool completed;
                try
                {

                    completed = Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(plan.WaitForDeploySeconds));

                    logger.Inspect(nameof(completed), completed);

                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Deploy", Explanation = "Mission deploy performed", Source = "Mission" });


                }
                catch (AggregateException cause)
                {

                    logger.Error(cause, "Mission deployment failed");

                    result.Successful = false;
                    foreach (var ex in cause.InnerExceptions)
                        result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Deployment", Explanation = ex.Message, Source = "Mission" });

                    return;
                }
                catch (Exception cause)
                {

                    logger.Error(cause, "Mission deployment failed");

                    result.Successful = false;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Deployment", Explanation = cause.Message, Source = "Mission" });

                    return;

                }

                if (!completed)
                {
                    result.Successful = false;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Deployment", Explanation = "Deployment did not complete in the required amount of time", Source = "Mission" });
                }

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        private void _start(IPlan plan, Result result)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var ready = plan.Deployments.Count(u => u.HasInstalled);
                logger.InfoFormat("{0} of {1} unit(s) successfully installed and are ready to start", ready, plan.Deployments.Count);

                var notInstalled = plan.Deployments.Count(u => !u.HasInstalled);
                logger.InfoFormat("There are {0} unit(s) that failed to install and are not ready to start", notInstalled);

                logger.InfoFormat("This mission is running under AllAppliancesMustDeploy conditions? {0}", plan.AllAppliancesMustDeploy);

                if (notInstalled > 0 && plan.AllAppliancesMustDeploy)
                    return;



                // *****************************************************************
                logger.Debug("Attempting to check if start is required");
                if (!Plan.StartAppliances)
                {
                    logger.Debug("Skipping Start per Plan.StartAppliances=false");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Start", Explanation = "Skipping Start per Plan.StartAppliances=false", Source = "Mission" });
                    return;
                }



                logger.Debug("Attempting to create and start process managers for each deployment unit");
                foreach (var unit in plan.Deployments.Where(u => u.HasInstalled))
                {

                    logger.LogObject(nameof(unit), unit);

                    var app = ApplianceFactory.Create(plan, unit);
                    Appliances.Add(app);

                    app.Start();


                    // *****************************************************************
                    if (unit.WaitForStart)
                    {
                        logger.DebugFormat("Attempting to wait for appliance ({0}) to start", unit.Alias);
                        app.WaitForStart();
                    }

                }


                result.Successful = true;
                result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Start", Explanation = "Mission start performed", Source = "Mission" });


            }
            catch (AggregateException cause)
            {

                logger.Error(cause, "Mission Start failed");

                result.Successful = false;
                foreach (var ex in cause.InnerExceptions)
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Start", Explanation = ex.Message, Source = "Mission" });

            }
            catch (Exception cause)
            {

                logger.Error(cause, "Mission Start failed");

                result.Successful = false;
                result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Start", Explanation = cause.Message, Source = "Mission" });

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        private void _stop(IPlan plan, Result result)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to check for empty Mission");
                if (Appliances.Count == 0)
                {
                    logger.Debug("Stopping empty Mission");
                    result.Successful = true;
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Empty Mission stop performed", Source = "Mission" });
                    return;
                }



                // *****************************************************************
                logger.Debug("Attempting to stop each appliance");
                foreach (var app in Appliances.Where(a => a.HasStarted))
                    app.Stop();



                // *****************************************************************
                logger.Debug("Attempting to wait for stop");
                var expired = DateTime.Now + TimeSpan.FromSeconds(plan.WaitForStopSeconds);
                while (expired > DateTime.Now)
                {

                    var stopped = Appliances.All(a => a.HasStopped);
                    if (stopped)
                        break;

                    Thread.Sleep(100);

                }



                // *****************************************************************
                logger.Debug("Attempting to dispose of each appliance");
                foreach (var mgr in Appliances)
                    mgr.Dispose();

               

                // *****************************************************************
                logger.Debug("Attempting to clear the Appliances");
                Appliances.Clear();


                result.Successful = true;
                result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Info, Group = "Stop", Explanation = "Mission stop performed", Source = "Mission" });


            }
            catch (AggregateException cause)
            {

                logger.Error(cause, "Mission Stop failed");

                result.Successful = false;
                foreach (var ex in cause.InnerExceptions)
                    result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Stop", Explanation = ex.Message, Source = "Mission" });

            }
            catch (Exception cause)
            {

                logger.Error(cause, "Mission Stop failed");

                result.Successful = false;
                result.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Error, Group = "Stop", Explanation = cause.Message, Source = "Mission" });

            }
            finally
            {
                logger.LeaveMethod();
            }

        }


    }


}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Configuration;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One;

public class OneService : BackgroundService
{


    private IContainer TheContainer { get; set; }
    private MissionObserver TheObserver { get; set; }


    public override async Task StartAsync( CancellationToken cancellationToken )
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();



            try
            {


                // *****************************************************************
                logger.Debug("Attempting to create Autofac container builder");
                var builder = new ContainerBuilder();



                // *****************************************************************
                logger.Debug("Attempting to bind to HostModule");
                var module = Program.TheConfiguration.Get<OneMissionModule>();
                builder.RegisterModule(module);

                logger.LogObject( nameof(module), module );



                // *****************************************************************
                logger.Debug("Attempting to check if Mission running under orchestration");
                if( module.UnderOrchestration )
                {

                    logger.Debug("Delete Mission Plan JSON file");
                    var missionPlanFile = new FileInfo($"{module.OneRoot}{Path.DirectorySeparatorChar}mission-plan.json");
                    if( missionPlanFile.Exists )
                        missionPlanFile.Delete();

                    logger.Debug("Delete Mission Status JSON file");
                    var missionStatusFile = new FileInfo($"{module.OneRoot}{Path.DirectorySeparatorChar}mission-status.json");
                    if( missionStatusFile.Exists )
                        missionStatusFile.Delete();

                }



                // *****************************************************************
                logger.Debug("Attempting to register configuration root");
                builder.RegisterInstance(Program.TheConfiguration)
                    .As<IConfigurationRoot>()
                    .SingleInstance();



                // *****************************************************************
                logger.Debug("Attempting to build container");
                TheContainer = builder.Build();



                // *****************************************************************
                logger.Debug("Attempting to start services that require starting");
                var startables = TheContainer.Resolve<IEnumerable<IRequiresStart>>();
                foreach (var rs in startables)
                {
                    logger.Inspect("Startable Type", rs.GetType().FullName);
                    await rs.Start();
                }


            }
            catch (Exception cause)
            {
                logger.Error(cause, "Container build failed");
                return;
            }



            try
            {

                // *****************************************************************
                logger.Debug("Attempting to resolve the Mission observer");
                TheObserver = TheContainer.Resolve<MissionObserver>();

            }
            catch (Exception cause)
            {
                logger.Error(cause, "Observer build failed");
                return;
            }



            try
            {

                // *****************************************************************
                logger.Debug("Attempting to start the Mission observer");
                TheObserver.Start();

            }
            catch (Exception cause)
            {
                logger.Error(cause, "Observer start failed");
                return;
            }





            // *****************************************************************
            await base.StartAsync(cancellationToken);


        }
        finally
        {
            logger.LeaveMethod();
        }


    }


    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {


        while( !stoppingToken.IsCancellationRequested )
        {

            try
            {

                await TheObserver.Check();

                await Task.Delay(100, stoppingToken);

            }
            catch (Exception cause)
            {
                using var logger = this.GetLogger();
                logger.Error(cause, "Unhandled exception caught in Execute loop");
            }


        }

    }


    public override async Task StopAsync( CancellationToken cancellationToken )
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();


            try
            {

                // *****************************************************************
                logger.Debug("Attempting to stop Mission observer");
                TheObserver?.Stop();


            }
            catch ( Exception cause )
            {
                logger.Error( cause, "Observer stop failed" );
            }


            try
            {

                // *****************************************************************
                logger.Debug("Attempting to disposing of container");
                TheContainer?.Dispose();

            }
            catch (Exception cause)
            {
                logger.Error(cause, "Container shutdown failed");
            }


            // *****************************************************************
            await base.StopAsync(cancellationToken);


        }
        finally
        {
            logger.LeaveMethod();
        }


    }


}
using Autofac;
using Fabrica.One.Orchestrator.Aws.Configuration;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One.Aws.Service;

#nullable disable
public class AwsService: BackgroundService
{


    private IContainer TheContainer { get; set; }
    private MissionOrchestrator TheOrchestrator { get; set; }


    public override async Task StartAsync(CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to create Autofac container builder");
            var builder = new ContainerBuilder();



            // *****************************************************************
            logger.Debug("Attempting to bind to HostModule");
            var module = Program.TheConfiguration.Get<OneOrchestratorModule>();
            builder.RegisterModule(module);

            logger.LogObject(nameof(module), module);



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
            TheOrchestrator = TheContainer.Resolve<MissionOrchestrator>();

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Observer build failed");
            return;
        }


        // *****************************************************************
        await base.StartAsync(cancellationToken);


    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {


        while( !stoppingToken.IsCancellationRequested )
        {

            try
            {

                await TheOrchestrator.CheckForUpdatedPlan();

                await Task.Delay(100, stoppingToken);

            }
            catch (Exception cause)
            {
                using var logger = this.GetLogger();
                logger.Error(cause, "Unhandled exception caught in Execute loop");
            }


        }

    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();


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



}
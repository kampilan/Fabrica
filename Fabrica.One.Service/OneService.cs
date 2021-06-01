using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One
{


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
                    var module = Program.TheConfiguration.Get<TheModule>();
                    builder.RegisterModule(module);

                    logger.LogObject( nameof(module), module );



                    // *****************************************************************
                    logger.Debug("Attempting to register configuration root");
                    builder.RegisterInstance(Program.TheConfiguration)
                        .As<IConfigurationRoot>()
                        .SingleInstance();



                    // *****************************************************************
                    logger.Debug("Attempting to build container");
                    TheContainer = builder.Build();



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

                await TheObserver.Check();

                await Task.Delay( 100, stoppingToken );

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


}

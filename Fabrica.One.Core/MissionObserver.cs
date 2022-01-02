using System;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Plan;
using Fabrica.Watch;

namespace Fabrica.One
{

    
    public class MissionObserver
    {


        public MissionObserver( ILifetimeScope root )
        {

            Root = root;
        }

        private ILifetimeScope Root { get; }
        private ILifetimeScope CurrentScope{ get; set; }

        private Mission CurrentMission { get; set; }

        public void Start()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                if( CurrentScope != null )
                {
                    logger.Info("Mission already runnning");
                    return;
                }


                try
                {

                    CurrentScope = Root.BeginLifetimeScope();

                    CurrentMission = CurrentScope.Resolve<Mission>();

                }
                catch( Exception cause )
                {
                    logger.Error( cause, "Mission acquisition failed." );
                    throw;
                }
                


                var result = CurrentMission.Run();

                if( !result.Successful )
                {
                    var ev = logger.CreateEvent(Level.Error, "MissionObserver Start failed.", result);
                    logger.LogEvent( ev );
                    throw new Exception( "Mission start failed.");
                }


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public async Task Check()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var source = CurrentScope.Resolve<IPlanSource>();

                var updated = await source.HasUpdatedPlan();

                logger.Inspect(nameof(updated), updated);

                if( updated )
                {
                    Stop();
                    Start();
                }

            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public async Task Reload()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var source = CurrentScope.Resolve<IPlanSource>();

                logger.Debug("Attempting to request Reload");
                await source.Reload();

            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public void Stop()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                CurrentMission?.Terminate();
                CurrentMission = null;

                CurrentScope?.Dispose();
                CurrentScope = null;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }




    }


}

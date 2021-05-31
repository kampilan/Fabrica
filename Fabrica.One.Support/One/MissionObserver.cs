using System;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Plan;
using Fabrica.Watch;

namespace Fabrica.One
{

    
    public class MissionObserver
    {


        public MissionObserver( ILifetimeScope scope )
        {

            Scope = scope;
        }

        private ILifetimeScope Scope { get; }
        private Mission CurrentMission { get; set; }

        public void Start()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                try
                {

                    CurrentMission = Scope.Resolve<Mission>();

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


        public void Stop()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                CurrentMission?.Terminate();

            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public async Task Check()
        {

            var source = Scope.Resolve<IPlanSource>();
            var updated = await source.HasUpdatedPlan();

            if ( updated )
            {
                Stop();
                Start();
            }

        }


    }


}

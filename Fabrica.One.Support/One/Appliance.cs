using System;
using System.Diagnostics;
using Fabrica.One.Plan;
using Fabrica.Utilities.Process;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.One
{



    public class Appliance : IAppliance
    {


        public Appliance([NotNull] DeploymentUnit unit)
        {

            Unit = unit;

            Controller = new FileSignalController(FileSignalController.OwnerType.Host, unit.ExecutionPath);

        }


        public DeploymentUnit Unit { get; set; }

        private ISignalController Controller { get; set; }

        private Process TheProcess { get; set; }



        public bool HasStarted => Controller.HasStarted;
        public bool HasStopped => Controller.HasStopped;


        public void Start()
        {

            using var logger = this.EnterMethod();


            logger.LogObject(nameof(Unit), Unit);


            // *****************************************************************
            logger.Debug("Attempting to cleanup this host");
            _cleanup();



            // *****************************************************************
            logger.Debug("Attempting to start appliance process");
            if (TheProcess == null)
            {

                logger.Debug("Process does not exist. Creating new");

                var startInfo = new ProcessStartInfo
                {
                    FileName = Unit.ExecutionCommand,
                    Arguments = Unit.ExecutionArguments,
                    UseShellExecute = Unit.ShowWindow,
                    CreateNoWindow = true
                };

                TheProcess = Process.Start(startInfo);
            }
            else
            {
                logger.Debug("Process existing. Reusing");
                TheProcess.Start();
            }


        }


        public bool WaitForStart(TimeSpan duration)
        {

            var until = DateTime.Now + duration;

            while (until > DateTime.Now)
            {

                if (Controller.HasStarted)
                    return true;

                if (HasStopped)
                    return false;

            }

            return false;

        }


        public void Stop()
        {

            using var logger = this.EnterMethod();

             Controller.RequestStop();

        }


        private void _cleanup()
        {

            Controller.Reset();

        }

        public void Dispose()
        {

            //           _cleanup();

        }


    }



}

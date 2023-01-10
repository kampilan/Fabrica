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

        public Appliance( [NotNull] IPlan plan, [NotNull] DeploymentUnit unit )
        {

            Plan = plan;
            Unit = unit;

            Controller = new FileSignalController(FileSignalController.OwnerType.Host, unit.InstallationLocation);

        }


        private IPlan Plan { get; }
        public DeploymentUnit Unit { get; }

        private ISignalController Controller { get; set; }

        private Process TheProcess { get; set; }

        public bool HasStarted => Controller.HasStarted;
        public bool HasStopped => Controller.HasStopped;


        public void Start()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



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
                        WorkingDirectory = Unit.InstallationLocation,
                        FileName         = Unit.ExecutionCommand,
                        Arguments        = Unit.ExecutionArguments,
                        UseShellExecute  = Unit.ShowWindow,
                        CreateNoWindow   = true
                    };

                    TheProcess = Process.Start(startInfo);
                }
                else
                {
                    logger.Debug("Process existing. Reusing");
                    TheProcess.Start();
                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public bool WaitForStart()
        {

            var until = DateTime.Now + TimeSpan.FromSeconds(Plan.WaitForStartSeconds);

            while (until > DateTime.Now)
            {

                if (Controller.HasStarted)
                    return true;

            }

            return false;

        }


        public void Stop()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                Controller.RequestStop();

            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        private void _cleanup()
        {

            Controller.Reset();

        }

        public void Dispose()
        {
            TheProcess?.Dispose();
            TheProcess = null;
        }


    }



}

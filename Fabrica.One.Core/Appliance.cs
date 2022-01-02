using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fabrica.One.Plan;
using Fabrica.Utilities.Process;
using Fabrica.Watch;
using JetBrains.Annotations;
using Medallion.Shell;

namespace Fabrica.One
{



    public class Appliance : IAppliance
    {


        public Appliance( [NotNull] DeploymentUnit unit )
        {

            Unit = unit ?? throw new ArgumentNullException(nameof(unit));

            Controller = new FileSignalController(FileSignalController.OwnerType.Host, unit.InstallationLocation);

        }


        public DeploymentUnit Unit { get; set; }

        private ISignalController Controller { get; set; }

        private Command TheCommand { get; set; }

        private List<string> StdOutput { get; } = new List<string>();
        private List<string> ErrorOutput { get; } = new List<string>();


        public bool HasStarted => Controller.HasStarted;
        public bool HasStopped => Controller.HasStopped;


        public async Task Start()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(Unit), Unit);


                // *****************************************************************
                logger.Debug("Attempting to cleanup this host");
                _cleanup();



                // *****************************************************************
                logger.Debug("Attempting to start appliance process");

                void Config( Shell.Options o )
                {
                    o.StartInfo(si =>
                    {
                        si.WorkingDirectory = Unit.InstallationLocation;
                        si.CreateNoWindow = !Unit.ShowWindow;
                    });
                }

                TheCommand = Command.Run( Unit.ExecutionCommand, options: Config )
                    .RedirectTo(StdOutput)
                    .RedirectStandardErrorTo(ErrorOutput);


                await TheCommand.Task;


            }
            finally
            {
                logger.LeaveMethod();
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

        public string GetStdOutput()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                var sb = new StringBuilder();
                foreach (var line in StdOutput)
                    sb.AppendLine(line);

                var output = sb.ToString();

                return output;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        public string GetErrorOutput()
        {
            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                var sb = new StringBuilder();
                foreach (var line in ErrorOutput)
                    sb.AppendLine(line);

                var output = sb.ToString();

                return output;

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


                Controller.RequestStop();


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        private void _cleanup()
        {

            StdOutput.Clear();
            ErrorOutput.Clear();

            Controller.Reset();

        }

        public void Dispose()
        {

            //           _cleanup();

        }


    }



}

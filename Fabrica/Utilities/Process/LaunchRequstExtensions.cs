using System.Diagnostics;
using System.Text;
using Fabrica.Watch;

namespace Fabrica.Utilities.Process
{

    public static class LaunchRequstExtensions
    {


        public static LaunchResult Run( this ILaunchRequest request )
        {

            var logger = request.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.LogObject(nameof(request), request);

                

                // *****************************************************************
                logger.Debug("Attempting to build ProcessStartInfo");
                var capture = request.CaptureOutput && !request.UseShellExecute;
                var startInfo = new ProcessStartInfo
                {
                    FileName               = request.ExecutablePath,
                    Arguments              = request.Arguments,
                    WorkingDirectory       = request.WorkingDirectory,
                    UseShellExecute        = request.UseShellExecute,
                    CreateNoWindow         = !request.ShowWindow,
                    RedirectStandardOutput = capture,
                    RedirectStandardError  = capture
                };




                // *****************************************************************
                logger.Debug("Attempting to build Process");
                var process = new System.Diagnostics.Process
                {
                    EnableRaisingEvents = capture,
                    StartInfo           = startInfo
                };


                var result = new LaunchResult(process);
                var output = new StringBuilder();
                var error  = new StringBuilder();



                // *****************************************************************
                logger.Debug("Attempting to wire-up events");
                if( capture )
                {
                    process.OutputDataReceived += (s, e) => output.AppendLine(e.Data);
                    process.ErrorDataReceived  += (s, e) => error.AppendLine(e.Data);
                }



                // *****************************************************************
                logger.Debug("Attempting to start Process");    
                process.Start();
                
                if( capture )
                {
                    logger.Debug("Attempting to read output and error");
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }



                // *****************************************************************
                if( request.WaitForExit )
                {
                    logger.Debug("Attempting to wait for exit");
                    process.WaitForExit();
                }



                // *****************************************************************
                logger.Debug("Attempting to update result");
                result.Output = output.ToString();
                result.Error  = error.ToString();



                // *****************************************************************
                logger.LogObject(nameof(result), result);
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

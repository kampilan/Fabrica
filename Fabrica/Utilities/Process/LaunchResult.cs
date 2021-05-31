namespace Fabrica.Utilities.Process
{


    public class LaunchResult
    {


        public LaunchResult(System.Diagnostics.Process process)
        {
            TheProcess = process;
        }

        private System.Diagnostics.Process TheProcess { get; }


        public string Output { get; set; } = "";
        public string Error { get; set; } = "";

        public bool Exited => TheProcess?.HasExited??true;
        public int ExitCode => TheProcess?.ExitCode??0;


    }


}

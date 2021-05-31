namespace Fabrica.Utilities.Process
{



    public class LaunchRequest : ILaunchRequest
    {


        public bool CaptureOutput { get; set; }
        public bool WaitForExit { get; set; }

        public string WorkingDirectory { get; set; } = "";

        public string ExecutablePath { get; set; } = "";
        public string Arguments { get; set; } = "";

        public bool UseShellExecute { get; set; }
        public bool ShowWindow { get; set; }


        public void SetWorkingDirectory(string template, params object[] args)
        {

            var working = string.Format(template, args);

            WorkingDirectory = working;

        }

        public void SetExecutable( bool useShell, bool showWindow, string template, params object[] args)
        {

            var executable = string.Format(template, args);

            ExecutablePath  = executable;
            UseShellExecute = useShell;
            ShowWindow      = showWindow;

        }

        public void SetArguments( string template, params object[] args )
        {

            var arguments = string.Format(template, args);

            Arguments = arguments;

        }


    }



}

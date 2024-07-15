namespace Fabrica.Utilities.Process
{
    public interface ILaunchRequest
    {
        bool CaptureOutput { get; set; }
        bool WaitForExit { get; set; }
        string WorkingDirectory { get; set; }
        string ExecutablePath { get; set; }
        string Arguments { get; set; }
        bool UseShellExecute { get; set; }
        bool ShowWindow { get; set; }
    }
}
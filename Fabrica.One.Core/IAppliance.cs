using System;
using System.Threading.Tasks;
using Fabrica.One.Plan;

namespace Fabrica.One
{


    public interface IAppliance: IDisposable
    {

        DeploymentUnit Unit { get; }


        bool HasStarted { get; }
        bool HasStopped { get; }

        Task Start();
        bool WaitForStart(TimeSpan duration);

        string GetStdOutput();
        string GetErrorOutput();

        void Stop();

    }


}

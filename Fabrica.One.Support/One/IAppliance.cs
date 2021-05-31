using System;
using Fabrica.One.Plan;

namespace Fabrica.One
{


    public interface IAppliance: IDisposable
    {

        DeploymentUnit Unit { get; }


        bool HasStarted { get; }
        bool HasStopped { get; }

        void Start();
        bool WaitForStart(TimeSpan duration);
        
        
        void Stop();

    }


}

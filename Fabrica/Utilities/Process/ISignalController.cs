using System;

namespace Fabrica.Utilities.Process
{


    public interface ISignalController
    {


        void Started();
        bool HasStarted { get; }
        bool WaitForStarted( TimeSpan interval );

        void RequestStop();
        bool MustStop { get; }
        bool WaitForMustStop( TimeSpan interval );

        void Stopped();
        bool HasStopped { get; }
        bool WaitForStopped( TimeSpan interval );

        void Reset();


    }


}

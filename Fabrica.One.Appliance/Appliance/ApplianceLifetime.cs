using Fabrica.Utilities.Container;
using Fabrica.Utilities.Process;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One.Appliance
{

    public class ApplianceLifetime : IRequiresStart
    {

        public ApplianceLifetime( IHostApplicationLifetime lifetime, ISignalController controller )
        {

            Lifetime   = lifetime;
            Controller = controller;

            Controller.Reset();

        }

        private IHostApplicationLifetime Lifetime { get; }
        private ISignalController Controller { get; }

        public Task Start()
        {

            Lifetime.ApplicationStarted.Register(Controller.Started);
            Lifetime.ApplicationStopped.Register(Controller.Stopped);

            Task.Run(_run);

            return Task.CompletedTask;

        }

        private void _run()
        {

            while( !Controller.WaitForMustStop(TimeSpan.FromSeconds(60)) )
            {
            }

            Lifetime.StopApplication();

        }


    }

}

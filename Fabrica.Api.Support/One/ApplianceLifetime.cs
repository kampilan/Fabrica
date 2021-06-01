using System;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Utilities.Process;
using Microsoft.Extensions.Hosting;

namespace Fabrica.Api.Support.One
{


    public class ApplianceLifetime : IStartable
    {

        public ApplianceLifetime( IHostApplicationLifetime lifetime, ISignalController controller )
        {

            Lifetime   = lifetime;
            Controller = controller;

            Controller.Reset();

        }

        private IHostApplicationLifetime Lifetime { get; }
        private ISignalController Controller { get; }

        public void Start()
        {

            Lifetime.ApplicationStarted.Register(() => Controller.Started());
            Lifetime.ApplicationStopped.Register(() => Controller.Stopped());

            Task.Run(_run);

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

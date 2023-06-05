using Fabrica.Utilities.Process;
using Fabrica.Watch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fabrica.One.Appliance;

public class ApplianceConsoleLifetimeWithExit : ApplianceConsoleLifetime
{
 
    public ApplianceConsoleLifetimeWithExit( ISignalController controller, IOptions<ConsoleLifetimeOptions> options, IHostEnvironment environment, IHostApplicationLifetime applicationLifetime, IOptions<HostOptions> hostOptions) : base(options, environment, applicationLifetime, hostOptions)
    {

        Controller = controller;

    }

    private ISignalController Controller { get; }


    protected override void OnApplicationStarted()
    {

        using var logger = this.EnterMethod();

        Console.Out.WriteLine( "Appliance started. Press Ctrl+C to shut down." );
        
        logger.Info("Appliance started. Press Ctrl+C to shut down.");
        logger.InfoFormat("Hosting environment: {0}", Environment.EnvironmentName);
        logger.InfoFormat("Content root path: {0}", Environment.ContentRootPath);

    }

    protected override void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {

        using var logger = this.EnterMethod();

        // *****************************************************************
        logger.Debug("Attempting to exit appliance after respecting ctrl-c");

        Controller.RequestStop();

        e.Cancel = true;

    }

}
using Fabrica.Watch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Fabrica.One.Appliance;

public class ApplianceConsoleLifetimeWithExit : ApplianceConsoleLifetime
{
 
    public ApplianceConsoleLifetimeWithExit(IOptions<ConsoleLifetimeOptions> options, IHostEnvironment environment, IHostApplicationLifetime applicationLifetime, IOptions<HostOptions> hostOptions) : base(options, environment, applicationLifetime, hostOptions)
    {
    }

    protected override void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {

        using var logger = this.EnterMethod();

        // *****************************************************************
        logger.Debug("Attempting to exit appliance after respecting ctrl-c");

        e.Cancel = false;

    }
}
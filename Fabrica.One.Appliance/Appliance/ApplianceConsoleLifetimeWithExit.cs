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
        e.Cancel = false;
    }
}
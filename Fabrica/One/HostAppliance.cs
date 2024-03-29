using Microsoft.Extensions.Hosting;

namespace Fabrica.One;

public class HostAppliance(IHost host) : IAppliance
{

    private IHost App { get; } = host;

    public void Run()
    {
        App.Run();
    }

    public async Task RunAsync()
    {
        await App.RunAsync();
    }

}



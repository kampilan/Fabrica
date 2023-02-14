using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One;

public interface IBootstrap
{

    Task<IAppliance> Boot<TService>() where TService : class,IHostedService;

    bool AllowManualExit { get; set; }

    IConfiguration Configuration { get; set; }

    void ConfigureWatch();

}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One;

public interface IBootstrap
{

    Task<IAppliance> Boot<TService>() where TService : class,IHostedService;

    IConfiguration Configuration { get; set; }

    void ConfigureWatch();

}
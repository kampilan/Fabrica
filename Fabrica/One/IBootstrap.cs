using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fabrica.One;

public interface IBootstrap
{

    Task<IAppliance> Boot(string path = "");

    bool AllowManualExit { get; set; }

    IConfiguration Configuration { get; set; }

    void ConfigureWatch();

}
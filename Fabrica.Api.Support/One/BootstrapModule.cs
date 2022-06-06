using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Api.Support.One;

public abstract class BootstrapModule
{


    public string Environment { get; set; } = "Development";

    public bool AllowAnyIp { get; set; } = false;
    public int ListeningPort { get; set; } = 8080;

    public string MissionName { get; set; } = "";
    public bool RunningAsMission => !string.IsNullOrWhiteSpace(MissionName);

    public bool RequiresAuthentication { get; set; } = true;

    public IConfiguration Configuration { get; set; }

    public virtual Task OnConfigured()
    {
        return Task.CompletedTask;
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
    }

    public virtual void ConfigureContainer(ContainerBuilder builder)
    {
    }

    public virtual void ConfigureWebApp( IApplicationBuilder app )
    {

    }

}
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Watch;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Container;

public class FabricaServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
{

    private readonly Action<ContainerBuilder> _configurationAction;
    private readonly ContainerBuildOptions _containerBuildOptions = ContainerBuildOptions.None;


    public FabricaServiceProviderFactory( ContainerBuildOptions containerBuildOptions, Action<ContainerBuilder> configurationAction = null) : this(configurationAction) => _containerBuildOptions = containerBuildOptions;

    public FabricaServiceProviderFactory(Action<ContainerBuilder> configurationAction = null) => _configurationAction = configurationAction ?? (builder => { });

    public ContainerBuilder CreateBuilder(IServiceCollection services)
    {

        using var logger = this.EnterMethod();


        var builder = new ContainerBuilder();

        builder.Populate(services);

        _configurationAction(builder);

        return builder;

    }

    public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
    {

        if (containerBuilder == null)
            throw new ArgumentNullException(nameof(containerBuilder));


        using var logger = this.EnterMethod();


        var container = containerBuilder.Build(_containerBuildOptions);

        return new FabricaServiceProvider(container);


    }


}
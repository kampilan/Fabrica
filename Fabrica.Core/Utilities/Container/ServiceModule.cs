using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Utilities.Container;

public abstract class ServiceModule: Module
{

    protected ServiceCollection Services { get; } = new ();
    protected ContainerBuilder Builder { get; private set; } = new();

    protected sealed override void Load( ContainerBuilder builder )
    {

        Builder = builder;

        Load();

        Builder.Populate(Services);

    }

    protected abstract void Load();


}
using Autofac;
using Fabrica.Watch;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Container;

public class FabricaServiceScopeFactory: IServiceScopeFactory
{

    public FabricaServiceScopeFactory(ILifetimeScope rootScope, Action<ContainerBuilder> builder )
    {
        RootScope    = rootScope;
        ScopeBuilder = builder;
    }

    private ILifetimeScope RootScope { get; }

    private Action<ContainerBuilder> ScopeBuilder { get; }

    public IServiceScope CreateScope()
    {

        using var logger = this.EnterMethod();


        var scope = RootScope.BeginLifetimeScope(ScopeBuilder);

        var ss = new FabricaServiceScope(scope);

        return ss;

    }

}
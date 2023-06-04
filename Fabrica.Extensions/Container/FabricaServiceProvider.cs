using Autofac.Core;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Container;

public partial class FabricaServiceProvider : IServiceProvider, ISupportRequiredService, IServiceProviderIsService, IDisposable, IAsyncDisposable
{

    private readonly ILifetimeScope _lifetimeScope;

    private bool _disposed;

    public FabricaServiceProvider(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public object GetRequiredService(Type serviceType)
    {
        return _lifetimeScope.Resolve(serviceType);
    }

    public bool IsService(Type serviceType) => _lifetimeScope.ComponentRegistry.IsRegistered(new TypedService(serviceType));

    public object? GetService(Type serviceType)
    {
        return _lifetimeScope.ResolveOptional(serviceType);
    }

    public ILifetimeScope LifetimeScope => _lifetimeScope;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            if (disposing)
            {
                _lifetimeScope.Dispose();
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            await _lifetimeScope.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }


}
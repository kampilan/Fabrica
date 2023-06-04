using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Container;

public class FabricaServiceScope: IServiceScope, IAsyncDisposable
{


    public FabricaServiceScope(ILifetimeScope scope)
    {
        _provider = new AutofacServiceProvider(scope);
    }

    private bool _disposed;


    private readonly AutofacServiceProvider _provider;
    public IServiceProvider ServiceProvider => _provider;


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if( !_disposed )
        {
            _disposed = true;

            if (disposing)
                _provider.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {

        if( !_disposed )
        {
            _disposed = true;
            await _provider.DisposeAsync().ConfigureAwait(false);
        }

    }


}
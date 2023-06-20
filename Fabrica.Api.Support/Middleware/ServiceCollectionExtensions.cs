using Fabrica.Utilities.Container;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Api.Support.Middleware;

public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddDiagnosticMiddleware( this IServiceCollection collection, DiagnosticOptions? options = null )
    {

        collection.AddTransient(p =>
        {

            var corr = p.GetRequiredService<ICorrelation>();
            options = options ??= new DiagnosticOptions();

            var mw = new DiagnosticsMonitorMiddleware(corr, options);

            return mw;

        });


        return collection;


    }

}
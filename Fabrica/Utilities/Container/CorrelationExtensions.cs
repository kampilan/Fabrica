using System.Security.Claims;
using Autofac;

namespace Fabrica.Utilities.Container;

public static class CorrelationExtensions
{


    public static ContainerBuilder AddCorrelation( this ContainerBuilder builder )
    {

        builder.Register(_ => new Correlation())
            .As<ICorrelation>()
            .AsSelf()
            .InstancePerLifetimeScope();

        return builder;

    }

    public static ClaimsIdentity ToIdentity(this ICorrelation correlation)
    {
        var identity = correlation.Caller.Identity as ClaimsIdentity ?? new ClaimsIdentity();
        return identity;
    }

    public static bool TryGetAuthenticatedIdentity(this ICorrelation correlation, out ClaimsIdentity ci)
    {

        var caller = correlation.Caller;

        ci = null;

        if( caller?.Identity == null || !caller.Identity.IsAuthenticated )
            return false;

        if( caller.Identity is ClaimsIdentity cand )
        {
            ci = cand;
            return true;
        }

        return false;

    }


}
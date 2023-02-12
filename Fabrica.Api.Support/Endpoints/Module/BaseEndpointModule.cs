using System.Reflection;
using Fabrica.Models.Support;
using Humanizer;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class BaseEndpointModule : AbstractEndpointModule
{

    protected static string ExtractResource<T>() where T : class
    {

        var attr = typeof(T).GetCustomAttribute<ModelAttribute>();
        var path = attr is not null ? attr.Resource : "";

        if (string.IsNullOrWhiteSpace(path))
            path = typeof(T).Name.Pluralize().ToLowerInvariant();

        return path;

    }


    protected BaseEndpointModule()
    {

        if( GetType().GetCustomAttribute<ModuleRouteAttribute>() is { } mra )
            BasePath = mra.Path;

        if( GetType().GetCustomAttribute<ModulePolicyAttribute>() is { } mpa )
            RequireAuthorization(mpa.PolicyNames.ToArray());

    }


    protected BaseEndpointModule(string route)
    {

        BasePath = route;

        if (GetType().GetCustomAttribute<ModulePolicyAttribute>() is { } mpa)
            RequireAuthorization(mpa.PolicyNames.ToArray());

    }


}
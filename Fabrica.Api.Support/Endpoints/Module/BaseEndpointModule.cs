using System.Reflection;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Humanizer;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class BaseEndpointModule : AbstractEndpointModule
{

    static BaseEndpointModule()
    {

        Settings = new JsonSerializerSettings
        {
            ContractResolver = new ModelContractResolver(),
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

    }

    public static JsonSerializerSettings Settings { get; }

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

        if (GetType().GetCustomAttribute<ModuleRouteAttribute>() is { } attr)
            BasePath = attr.Path;

    }


    protected BaseEndpointModule(string route)
    {

        BasePath = route;

    }


}
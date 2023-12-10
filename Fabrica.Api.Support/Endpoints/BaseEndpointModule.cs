using System.Reflection;
using Fabrica.Models.Support;
using Fabrica.Rql.Parser;
using Humanizer;
using Microsoft.AspNetCore.Routing;
// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Endpoints;


public abstract class BaseEndpointModule : IEndpointModule
{

    internal object[] MetaData = Array.Empty<object>();

    internal string OpenApiDescription = string.Empty;

    internal string OpenApiName = string.Empty;

    internal string OpenApiSummary = string.Empty;

    internal string OpenApiDisplayName = string.Empty;

    internal string OpenApiGroupName = string.Empty;

    internal string[] Tags = Array.Empty<string>();

    internal bool RequiresAuthorization;

    internal string[] AuthorizationPolicyNames = Array.Empty<string>();

    internal string BasePath = string.Empty;

    public abstract void AddRoutes(IEndpointRouteBuilder app);

}

public abstract class BaseEndpointModule<T> : BaseEndpointModule where T : BaseEndpointModule<T>
{


    public T RequireAuthorization(params string[] policyNames)
    {
        RequiresAuthorization = true;
        AuthorizationPolicyNames = policyNames;
        return (T)this;
    }



    public T WithDescription(string description)
    {
        OpenApiDescription = description;
        return (T)this;
    }

    public T WithName(string name)
    {
        OpenApiName = name;
        return (T)this;
    }

    public T WithDisplayName(string displayName)
    {
        OpenApiDisplayName = displayName;
        return (T)this;
    }

    public T WithGroupName(string groupName)
    {
        OpenApiGroupName = groupName;
        return (T)this;
    }

    public T WithSummary(string summary)
    {
        OpenApiSummary = summary;
        return (T)this;
    }

    public T WithMetadata(params object[] items)
    {
        MetaData = items;
        return (T)this;
    }

    public T WithTags(params string[] tags)
    {
        Tags = tags;
        return (T)this;
    }


    protected void CheckOpenApiDefaults<TEntity>() where TEntity : class, IModel
    {

        if( Tags.Length == 0 )
        {
            var label = ExtractTag<TEntity>();
            WithTags(label);
        }

    }

    protected virtual string ExtractTag<TEntity>() where TEntity : class, IModel
    {

        var attr = typeof(TEntity).GetCustomAttribute<ModelAttribute>();

        var name = typeof(TEntity).Name;
        if (attr is not null && !string.IsNullOrWhiteSpace(attr.Resource))
            name = attr.Resource;

        var label = name.Pluralize();

        return label;

    }


    protected static string ExtractResource<TTarget>() where TTarget : class
    {

        var attr = typeof(TTarget).GetCustomAttribute<ModelAttribute>();
        var path = attr is not null ? attr.Resource : "";

        if (string.IsNullOrWhiteSpace(path))
            path = typeof(TTarget).Name.Pluralize().ToLowerInvariant();

        return path;

    }


    protected BaseEndpointModule()
    {

        if (GetType().GetCustomAttribute<ModuleRouteAttribute>() is { } mra)
            BasePath = mra.Path;

        if (GetType().GetCustomAttribute<ModulePolicyAttribute>() is { } mpa)
            RequireAuthorization(mpa.PolicyNames.ToArray());

    }


    protected BaseEndpointModule(string route)
    {

        BasePath = route;

        if (GetType().GetCustomAttribute<ModulePolicyAttribute>() is { } mpa)
            RequireAuthorization(mpa.PolicyNames.ToArray());

    }


}
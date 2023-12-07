using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Endpoints;

public class DeleteEndpointModule<TEntity> : BasePersistenceEndpointModule<DeleteEndpointModule<TEntity>> where TEntity : class, IModel
{


    protected DeleteEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected DeleteEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }



    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Delete", description: $"Delete {typeof(TEntity).Name} by UID"))
            .Produces(200)
            .Produces<ErrorResponseModel>(404);

    }


}
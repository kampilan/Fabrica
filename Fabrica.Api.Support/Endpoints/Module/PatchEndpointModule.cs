﻿
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class PatchEndpointModule<TEntity> : BasePersistenceEndpointModule where TEntity : class, IModel
{


    protected PatchEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected PatchEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Patch", description: $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

    }


}
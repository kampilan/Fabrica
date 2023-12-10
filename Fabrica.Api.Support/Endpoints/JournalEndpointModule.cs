
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Models;
using Fabrica.Models.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Support.Endpoints;

public abstract class JournalEndpointModule<TEntity> : BasePersistenceEndpointModule<JournalEndpointModule<TEntity>> where TEntity : class, IMutableModel
{


    protected JournalEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected JournalEndpointModule(string route) : base(route)
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        CheckOpenApiDefaults<TEntity>();


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithTags(Tags)
            .WithGroupName(OpenApiGroupName)
            .WithSummary("Journal")
            .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
            .Produces<List<AuditJournalModel>>();

    }


}
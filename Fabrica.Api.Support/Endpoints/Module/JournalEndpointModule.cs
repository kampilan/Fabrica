
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class JournalEndpointModule<TEntity> : BasePersistenceEndpointModule where TEntity : class, IMutableModel
{


    protected JournalEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");
        IncludeInOpenApi();

    }

    protected JournalEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");
        IncludeInOpenApi();

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Journal", description: $"{typeof(TEntity).Name} Audit Journal for given UID"))
            .Produces<List<AuditJournalModel>>();

    }


}

// ReSharper disable UnusedMember.Global

using System.Text.Json;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Patch;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Controllers;

public abstract class BasePatchController : BaseMediatorController
{


    protected BasePatchController(ICorrelation correlation, IModelMetaService meta, IMessageMediator mediator, IPatchResolver resolver, JsonSerializerOptions options ) : base(correlation, meta,mediator, options)
    {

        Resolver = resolver;

    }

    protected IPatchResolver Resolver { get; }


    protected virtual async Task<IActionResult> Send( IEnumerable<ModelPatch> source )
    {

        if (source == null) throw new ArgumentNullException(nameof(source));

        using var logger = EnterMethod();

        // *****************************************************************
        logger.Debug("Attempting to build patch set");
        var set = new PatchSet();
        set.Add(source);

        logger.Inspect(nameof(set.Count), set.Count);



        // *****************************************************************
        logger.Debug("Attempting to resolve patch set");
        var requests = Resolver.Resolve(set);



        // *****************************************************************
        logger.Debug("Attempting to send request via Mediator");
        var response = await Mediator.Send(requests);

        logger.Inspect(nameof(response.HasErrors), response.HasErrors);



        // *****************************************************************
        logger.Debug("Attempting to BatchResponse for success");
        response.EnsureSuccess();



        // *****************************************************************
        logger.Debug("Attempting to build result");
        var result = Ok();



        // *****************************************************************
        return result;

    }




}
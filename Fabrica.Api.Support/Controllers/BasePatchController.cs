using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Persistence.Patch;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Controllers;

public abstract class BasePatchController : BaseMediatorController
{


    protected BasePatchController(ICorrelation correlation, IMessageMediator mediator, IPatchResolver resolver) : base(correlation,mediator)
    {

        Resolver = resolver;

    }

    protected IPatchResolver Resolver { get; }


    protected virtual async Task<IActionResult> Send( [NotNull] IEnumerable<ModelPatch> source )
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
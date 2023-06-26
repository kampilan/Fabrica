
// ReSharper disable UnusedMember.Global

using Fabrica.Persistence.Patch;
using Fabrica.Watch;

namespace Fabrica.Mediator;

public static class MessageMediatorExtensions
{


    public static async Task<BatchResponse> Send( this IMessageMediator mediator, IEnumerable<PatchRequest> requests, bool stopOnFailure = true, CancellationToken token = default )
    {

        if (mediator == null) throw new ArgumentNullException(nameof(mediator));
        if (requests == null) throw new ArgumentNullException(nameof(requests));


        using var logger = mediator.EnterMethod();

        logger.Inspect(nameof(stopOnFailure), stopOnFailure);


        // *****************************************************************
        var responses = new BatchResponse();
        foreach (var request in requests)
        {

            logger.Debug("Attempting to apply {0} patch for Model: ({1}) Uid: ({2})", request.Source.Verb, request.Source.Model, request.Source.Uid);
                
            var response = await request.Apply(mediator);
            responses.Add(response);

            logger.Inspect(nameof(response.Ok), response.Ok);

            if (stopOnFailure && !response.Ok)
                return responses;

        }


        // *****************************************************************
        return responses;

    }

}
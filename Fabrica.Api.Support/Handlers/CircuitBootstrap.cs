using Autofac;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Components.Server.Circuits;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Handlers;

public class CircuitBootstrap: CircuitHandler
{


    public CircuitBootstrap(ILifetimeScope scope)
    {
        CircuitScope = scope;
    }

    private ILifetimeScope CircuitScope { get; }


    public override int Order => 1000;


    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();

        var currentStartable = "";
        try
        {

            // *****************************************************************
            logger.Debug("Attempting to resolve all components that require starting");
            var startables = CircuitScope.Resolve<IEnumerable<IRequiresCircuitStart>>();
            foreach (var c in startables)
            {
                currentStartable = c.GetType().FullName;
                logger.Inspect("Component Type", currentStartable);
                await c.Start();
            }

        }
        catch( Exception cause )
        {
            var ctx = new { FailedStartable = currentStartable };
            logger.ErrorWithContext(cause, ctx, "InitService failed during start");
            throw;
        }


    }


}
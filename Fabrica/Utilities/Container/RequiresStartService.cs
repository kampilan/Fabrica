﻿
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Watch;
using Microsoft.Extensions.Hosting;

namespace Fabrica.Utilities.Container;

public class RequiresStartService(ILifetimeScope root) : IHostedService
{

    protected ILifetimeScope RootScope { get; } = root;


    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();


        var currentStartable = "";
        try
        {
            // *****************************************************************
            logger.Debug("Attempting to resolve all components that require starting");
            var startables = RootScope.Resolve<IEnumerable<IRequiresStart>>();
            foreach (var c in startables)
            {
                currentStartable = c.GetType().FullName;
                logger.Inspect("Component Type", currentStartable);
                await c.Start();
            }

        }
        catch (Exception cause)
        {
            var ctx = new { FailedStartable = currentStartable };
            logger.ErrorWithContext( cause, ctx, "RequiresStartService failed during start");
            throw;
        }

    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();

        return Task.CompletedTask;

    }


}
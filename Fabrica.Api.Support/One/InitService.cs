using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.Extensions.Hosting;

namespace Fabrica.Api.Support.One;

public class InitService: IHostedService
{


    public InitService( ILifetimeScope root )
    {

        RootScope = root;

    }

    protected ILifetimeScope RootScope { get; }


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
            logger.ErrorWithContext( cause, ctx, "InitService failed durring start");
            throw;
        }

    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {

        using var logger = this.EnterMethod();

        return Task.CompletedTask;

    }


}
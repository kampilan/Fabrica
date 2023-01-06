// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Watch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ILogger = Fabrica.Watch.ILogger;

namespace Fabrica.Api.Support.One;

public static class ApplianceOneExtensions
{

    public static IHostBuilder UseApplianceConsoleLifetime(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, collection) => collection.AddSingleton<IHostLifetime, ApplianceConsoleLifetime>());
    }

}



public class ApplianceConsoleLifetime : IHostLifetime, IDisposable
{

    private readonly ManualResetEvent _shutdownBlock = new (false);
    private CancellationTokenRegistration _applicationStartedRegistration;
    private CancellationTokenRegistration _applicationStoppingRegistration;

    public ApplianceConsoleLifetime( IOptions<ConsoleLifetimeOptions> options, IHostEnvironment environment, IHostApplicationLifetime applicationLifetime, IOptions<HostOptions> hostOptions )
    {

        Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        HostOptions = hostOptions?.Value ?? throw new ArgumentNullException(nameof(hostOptions));

    }


    private ConsoleLifetimeOptions Options { get; }

    private IHostEnvironment Environment { get; }

    private IHostApplicationLifetime ApplicationLifetime { get; }

    private HostOptions HostOptions { get; }

    private ILogger GetLogger() => WatchFactoryLocator.Factory.GetLogger<ApplianceConsoleLifetime>();

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        if (!Options.SuppressStatusMessages)
        {
            _applicationStartedRegistration = ApplicationLifetime.ApplicationStarted.Register(state =>
                {
                    ((ApplianceConsoleLifetime)state).OnApplicationStarted();
                },
                this);
            _applicationStoppingRegistration = ApplicationLifetime.ApplicationStopping.Register(state =>
                {
                    ((ApplianceConsoleLifetime)state).OnApplicationStopping();
                },
                this);
        }


        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Console.CancelKeyPress += OnCancelKeyPress;

        // Console applications start immediately.
        return Task.CompletedTask;
    }

    private void OnApplicationStarted()
    {

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();

            logger.Info("Application started. Press Ctrl+C to shut down.");
            logger.InfoFormat("Hosting environment: {0}", Environment.EnvironmentName);
            logger.InfoFormat("Content root path: {0}", Environment.ContentRootPath);

        }
        finally
        {
            logger.LeaveMethod();
        }
            
    }

    private void OnApplicationStopping()
    {
        var logger = GetLogger();

        try
        {

            logger.EnterMethod();

            logger.Info("Application is shutting down...");

        }
        finally
        {
            logger.LeaveMethod();
        }

    }


    private void OnProcessExit(object sender, EventArgs e)
    {

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();


            ApplicationLifetime.StopApplication();
            if (!_shutdownBlock.WaitOne(HostOptions.ShutdownTimeout))
            {
                logger.Info("Waiting for the host to be disposed. Ensure all 'IHost' instances are wrapped in 'using' blocks.");
            }
            _shutdownBlock.WaitOne();
            System.Environment.ExitCode = 0;

        }
        finally
        {
            logger.LeaveMethod();
        }


    }

    private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
    }



    public Task StopAsync(CancellationToken cancellationToken)
    {
        // There's nothing to do here
        return Task.CompletedTask;
    }

    public void Dispose()
    {

        _shutdownBlock.Set();

        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        Console.CancelKeyPress -= OnCancelKeyPress;

        _applicationStartedRegistration.Dispose();
        _applicationStoppingRegistration.Dispose();

    }

}
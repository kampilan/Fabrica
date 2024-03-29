using Autofac;
using Fabrica.Container;
using Fabrica.One;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Bridges.MicrosoftImpl;
using Fabrica.Watch.Switching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Hosting;

public abstract class GenericHostBootstrap(): CorrelatedObject(new Correlation()), IBootstrap
{

    public IConfiguration Configuration { get; set; } = null!;

    public bool AllowManualExit { get; set; }

    public bool QuietLogging { get; set; } = false;

    public bool RealtimeLogging { get; set; } = false;
    public List<LocalSwitchConfig> RealtimeSwitches { get; set; } = [];

    public bool RelayLogging { get; set; } = false;


    public string WatchEventStoreUri { get; set; } = "";
    public string WatchDomainName { get; set; } = "";
    public int WatchPollingDurationSecs { get; set; } = 15;


    protected IHostBuilder Builder { get; set; } = null!;


    public virtual void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseQuiet();

        maker.Build();

    }

    public virtual async Task<IAppliance> Boot(string path = "")
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to call OnConfigured");
        await OnConfigured();

        logger.LogObject("Boostrap", this );



        // *****************************************************************
        logger.Debug("Attempting to create HostBuilder");
        Builder = Host.CreateDefaultBuilder();



        // *****************************************************************
        logger.Debug("Attempting to call ConfigureHost");
        ConfigureHost(Builder);



        // *****************************************************************
        logger.Debug("Attempting to Add Host Configuration ");
        Builder.ConfigureHostConfiguration(cfb => cfb.AddConfiguration(Configuration));



        // *****************************************************************
        logger.Debug("Attempting to Add App Configuration ");
        Builder.ConfigureAppConfiguration((cfb) => cfb.AddConfiguration(Configuration));



        // *****************************************************************
        logger.Debug("Attempting to Configure the Microsoft Logging bridge");
        Builder.ConfigureLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddProvider(new LoggerProvider());
            lb.SetMinimumLevel(LogLevel.Trace);
        });



        // *****************************************************************
        logger.Debug("Attempting to call ConfigureServices");
        Builder.ConfigureServices(sc =>
        {

            sc.AddHostedService<RequiresStartService>();


            using var inner = GetLogger();
            try
            {
                inner.Debug("Attempting to call ConfigureServices");
                ConfigureServices(sc);
            }
            catch (Exception cause)
            {
                inner.ErrorWithContext(cause, this, "Bootstrap ConfigureServices failed.");
                throw;
            }


        });



        // *****************************************************************
        logger.Debug("Attempting to call UseServiceProviderFactory");
        Builder.UseServiceProviderFactory(new FabricaServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(Configuration)
                .As<IConfiguration>()
                .SingleInstance();


            cb.AddCorrelation();


            using var inner = GetLogger();

            try
            {
                inner.Debug("Attempting to call ConfigureContainer");
                ConfigureContainer(cb);
            }
            catch (Exception cause)
            {
                inner.ErrorWithContext(cause, this, "Bootstrap ConfigureContainer failed.");
                throw;
            }


        }));



        // *****************************************************************
        logger.Debug("Attempting to build Host");
        var host = Builder.Build();



        // *****************************************************************
        logger.Debug("Attempting to create HostAppliance");
        var app = new HostAppliance(host);



        // *****************************************************************
        return app;


    }


    public virtual Task OnConfigured()
    {

        using var logger = EnterMethod();

        logger.Info("Base OnConfigured does nothing");

        return Task.CompletedTask;

    }


    public virtual void ConfigureHost( IHostBuilder builder )
    {

        using var logger = EnterMethod();

        
        logger.Info("Base ConfigureHost does nothing");

    }


    public virtual void ConfigureServices(IServiceCollection services)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureServices adds no services");


    }

    public virtual void ConfigureContainer(ContainerBuilder builder)
    {

        using var logger = EnterMethod();

        logger.Info("Base ConfigureContainer adds no services");

    }


}



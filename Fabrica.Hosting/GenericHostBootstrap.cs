using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Container;
using Fabrica.One;
using Fabrica.One.Appliance;
using Fabrica.Services;
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


    public string ApplicationLifetimeType { get; set; } = "FabricaOne";
    public string ApplicationBaseDirectory { get; set; } = string.Empty;
    public bool AllowManualExit { get; set; } = false;


    public IConfiguration Configuration { get; set; } = null!;

    public bool QuietLogging { get; set; } = false;

    public bool RealtimeLogging { get; set; } = false;
    public List<LocalSwitchConfig> RealtimeSwitches { get; set; } = [];

    public bool RelayLogging { get; set; } = false;


    public string WatchEventStoreUri { get; set; } = string.Empty;
    public string WatchDomainName { get; set; } = string.Empty;
    public int WatchPollingDurationSecs { get; set; } = 15;


    public string Environment { get; set; } = "Development";
    public string MissionName { get; set; } = string.Empty;
    public bool RunningAsMission => !string.IsNullOrWhiteSpace(MissionName);


    public string ApplianceId { get; set; } = string.Empty;
    public string ApplianceName { get; set; } = string.Empty;
    public string ApplianceBuild { get; set; } = string.Empty;
    public DateTime ApplianceBuildDate { get; set; } = DateTime.MinValue;
    public string ApplianceRoot { get; set; } = string.Empty;
    public DateTime ApplianceStartTime { get; set; } = DateTime.MinValue;



    protected IHostBuilder Builder { get; set; } = null!;


    public virtual void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseQuiet();

        maker.Build();

    }

    public async Task<IAppliance> Boot()
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
        logger.Debug("Attempting to configure ApplicationLifetime");
        switch (ApplicationLifetimeType.ToLowerInvariant())
        {
            case "fabricaone":
                Builder.UseFabricaOne(ApplicationBaseDirectory, AllowManualExit);
                break;
            case "systemd":
                Builder.UseSystemd();
                break;
        }



        // *****************************************************************
        logger.Debug("Attempting to call ConfigureServices");
        Builder.ConfigureServices(sc =>
        {

            sc.AddHostedService<RequiresStartService>();

        });



        // *****************************************************************
        logger.Debug("Attempting to call UseServiceProviderFactory");
        Builder.UseServiceProviderFactory(new FabricaServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(Configuration)
                .As<IConfiguration>()
                .SingleInstance();


            cb.AddCorrelation();


            var mc = Configuration.Get<MissionContext>();
            if (mc is not null)
            {

                cb.RegisterInstance(mc)
                    .AsSelf()
                    .As<IMissionContext>()
                    .SingleInstance();

                foreach (var pair in mc.ServiceEndpoints)
                {
                    var address = pair.Value.EndsWith("/") ? pair.Value : $"{pair.Value}/";
                    cb.AddServiceAddress(pair.Key, address);
                }

            }



            var inner = GetLogger();

            try
            {
                inner.Debug("Attempting to call Configure");

                var services = new ServiceCollection();
                
                Build( Builder, services, cb );

                cb.Populate(services);

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


    protected virtual Task OnConfigured()
    {

        using var logger = EnterMethod();

        logger.Info("Base OnConfigured does nothing");

        return Task.CompletedTask;

    }

    protected abstract void Build( IHostBuilder host, IServiceCollection services, ContainerBuilder builder );


}



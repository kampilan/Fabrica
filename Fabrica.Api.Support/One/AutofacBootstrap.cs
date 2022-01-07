using System.Drawing;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Configuration.Yaml;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Process;
using Fabrica.Watch;
using Fabrica.Watch.Bridges.MicrosoftImpl;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fabrica.Api.Support.One
{
   

    public abstract class AutofacBootstrap<TModule,TOptions> where TModule : Module where TOptions: IApplianceOptions
    {

        protected IConfiguration Configuration { get; private set; }

        protected TOptions Options { get; private set; }

        protected virtual void ConfigureApp(ConfigurationBuilder builder)
        {

            // *****************************************************************
            builder
                .AddYamlFile("configuration.yml", true)
                .AddJsonFile("environment.json", true)
                .AddJsonFile("mission.json", true);

        }

        protected virtual void ConfigureWatch()
        {

            // *****************************************************************
            var options = Configuration.Get<WatchMongoOptions>();
            var maker = WatchFactoryBuilder.Create();
            if (options.RealtimeLogging || string.IsNullOrWhiteSpace(options.WatchDomainName) || string.IsNullOrWhiteSpace(options.WatchEventStoreUri))
                maker.UseRealtime(Level.Debug, Color.LightPink);
            else
                maker.UseMongo(options);

            // *****************************************************************
            maker.Build();

        }

        protected virtual void ConfigureContainer(ContainerBuilder builder)
        {


            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                builder.RegisterInstance(Configuration)
                    .As<IConfiguration>()
                    .SingleInstance();



                // *****************************************************************
                logger.Debug("Attempting to build appliance module from Configuration");
                var module = Configuration.Get<TModule>();

                builder.RegisterModule(module);



                // *****************************************************************
                logger.Debug("Attempting to build and register Mission context from Configuration");
                var mission = Configuration.Get<MissionContext>();

                builder.RegisterInstance(mission)
                    .As<IMissionContext>()
                    .SingleInstance();



                // *****************************************************************
                logger.Debug("Attempting to register SignalController");
                builder.Register(c =>
                {

                    var comp = new FileSignalController(FileSignalController.OwnerType.Appliance);
                    return comp;

                })
                    .As<ISignalController>()
                    .As<IRequiresStart>()
                    .SingleInstance()
                    .AutoActivate();



                // *****************************************************************
                logger.Debug("Attempting to register ApplianceLifetime");

                builder.Register(c =>
                {

                    var hal = c.Resolve<IHostApplicationLifetime>();
                    var sc = c.Resolve<ISignalController>();

                    var comp = new ApplianceLifetime(hal, sc);

                    return comp;

                })
                    .AsSelf()
                    .As<IRequiresStart>()
                    .SingleInstance()
                    .AutoActivate();


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public async Task Run()
        {

            var configBuilder = new ConfigurationBuilder();
            ConfigureApp(configBuilder);

            Configuration = configBuilder.Build();

            ConfigureWatch();

            Options = Configuration.Get<TOptions>();

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var hb = new HostBuilder()
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .UseEnvironment(Options.Environment)
                    .ConfigureLogging(lb =>
                    {
                        lb.ClearProviders();
                        lb.AddProvider(new LoggerProvider());
                        lb.SetMinimumLevel(LogLevel.Trace);
                    })
                    .ConfigureContainer<ContainerBuilder>(ConfigureContainer);


                if (Options.RunningAsMission)
                    hb.UseApplianceConsoleLifetime();
                else
                    hb.UseConsoleLifetime();


                using (var host = hb.Build())
                    await host.RunAsync();


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


    }


}

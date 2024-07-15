
using System.Drawing;
using Fabrica.Configuration.Yaml;
using Fabrica.Watch;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fabrica.One.Orchestrator.Aws;

public class Program
{


    public static IConfigurationRoot TheConfiguration { get; set; } = null!;


    public static async Task Main(string[] args)
    {


        Console.WriteLine("Fabrica One AWS Orchestrator Service");
        Console.WriteLine("Pond Hawk Technologies Inc. (c) 2024");
        Console.WriteLine("");

        Console.WriteLine("Press Ctrl-C to Stop");

        try
        {

            var cfgBuilder = new ConfigurationBuilder()
                .AddYamlFile("configuration.yml")
                .AddEnvironmentVariables();

            TheConfiguration = cfgBuilder.Build();

        }
        catch (Exception cause)
        {
            Console.WriteLine("Load Configuration failed.");
            Console.WriteLine(cause);
            return;
        }



        try
        {


            // *****************************************************************
            var options = TheConfiguration.Get<WatchMongoOptions>();
            var builder = WatchFactoryBuilder.Create();
            if (options.RealtimeLogging || string.IsNullOrWhiteSpace(options.WatchDomainName) || string.IsNullOrWhiteSpace(options.WatchEventStoreUri))
            {
                builder.UseRealtime(Level.Debug, Color.LightBlue);
            }
            else
                builder.UseMongo(options);



            // *****************************************************************
            builder.Build();

        }
        catch (Exception cause)
        {
            Console.WriteLine("Watch Build failed.");
            Console.WriteLine(cause);
            return;
        }



        await CreateHostBuilder(args).Build().RunAsync();


    }


    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .UseSystemd()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<ILoggerFactory>(new Fabrica.Watch.Bridges.MicrosoftImpl.LoggerFactory());
            services.AddHostedService<AwsService>();
        });


}
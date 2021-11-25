using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Fabrica.Configuration.Yaml;
using Fabrica.Watch;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fabrica.One
{

    
    public class Program
    {
   

        public static IConfigurationRoot TheConfiguration { get; set; }


        public static async Task Main(string[] args)
        {


            var path = Assembly.GetExecutingAssembly().Location;
            var info = new FileInfo(path);

            Directory.SetCurrentDirectory(info.DirectoryName);


            // *****************************************************************
            var headless = Console.OpenStandardOutput() == System.IO.Stream.Null;
            if (!headless)
            {

                Console.Clear();
                Console.WriteLine("Fabrica One Orchestration Service");
                Console.WriteLine("The Kampilan Group Inc. (c) 2020");
                Console.WriteLine("");

                Console.WriteLine("Press Ctrl-C to Stop");

            }



            try
            {

                var cfgBuilder = new ConfigurationBuilder()
                    .AddYamlFile("configuration.yml");

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
                    builder.UseRealtime(Level.Debug, Color.LightPink);
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


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ILoggerFactory>(new Watch.Bridges.MicrosoftImpl.LoggerFactory());
                    services.AddHostedService<OneService>();
                });


    }


}

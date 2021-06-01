using System;
using System.IO;
using System.Reflection;
using Fabrica.Configuration.Yaml;
using Fabrica.Watch;
using Fabrica.Watch.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fabrica.One
{
    public class Program
    {


        public static IConfigurationRoot TheConfiguration { get; set; }


        public static void Main(string[] args)
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



            CreateHostBuilder(args).Build().Run();


        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ILoggerFactory>(new Watch.Bridges.MicrosoftImpl.LoggerFactory());
                    services.AddHostedService<OneService>();
                });


    }

}

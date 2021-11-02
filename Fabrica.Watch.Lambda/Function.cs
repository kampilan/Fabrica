using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using System;
using System.Threading.Tasks;
using Fabrica.Configuration.Yaml;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Sink;
using Microsoft.Extensions.Configuration;

// This project specifies the serializer used to convert Lambda event into .NET classes in the project's main 
// main function. This assembly register a serializer for use when the project is being debugged using the
// AWS .NET Mock Lambda Test Tool.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Fabrica.Watch.Lambda
{
    public class Function
    {
        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main(string[] args)
        {


            var builder = new ConfigurationBuilder();
            builder.AddYamlFile("Configuration.yml");

            var config = builder.Build();
            var options = config.Get<WatchMongoOptions>();

            
            var maker = new WatchFactoryBuilder();
            maker.UseMongo(options);

            maker.Build();
            
            
            Action<string,string,string, ILambdaContext> func = FunctionHandler;
            using(var handlerWrapper = HandlerWrapper.GetHandlerWrapper(func, new DefaultLambdaJsonSerializer()))
            using(var bootstrap = new LambdaBootstrap(handlerWrapper))
            {
                await bootstrap.RunAsync();
            }
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        ///
        /// To use this handler to respond to an AWS event, reference the appropriate package from 
        /// https://github.com/aws/aws-lambda-dotnet#events
        /// and change the string input parameter to the desired event type.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static void FunctionHandler(string category, string title, string message, ILambdaContext context)
        {

            var logger = WatchFactoryLocator.Factory.GetLogger(category);

            var ev = logger.CreateEvent( Level.Debug, title, PayloadType.Text, message );

            logger.LogEvent( ev );

        }


    }

}

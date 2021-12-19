using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.Util;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fabrica.One.Plan
{

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class AppConfigPlanSource: AbstractPlanSource, IPlanSource, IRequiresStart
    {


        private class Settings
        {

            public string Application { get; set; } = "";
            public string Environment { get; set; } = "";
            public string Configuration { get; set; } = "";

        }



        public AppConfigPlanSource( IAmazonAppConfig client )
        {
            Client = client;
        }

        private IAmazonAppConfig Client { get; }


        public bool RunningOnEC2 { get; set; } = true;

        public string Application { get; set; } = "";
        public string Environment { get; set; } = "";
        public string Configuration { get; set; } = "";
        public string ClientId { get; set; } = ShortGuid.NewGuid();


        public async Task Start()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(Application), Application);
                logger.Inspect(nameof(Environment), Environment);
                logger.Inspect(nameof(Configuration), Configuration);



                // *****************************************************************
                logger.Debug("Attempting to check for set Application. Indicating manual config");
                if( !string.IsNullOrWhiteSpace(Application) )
                {
                    logger.Debug("Manually configured");
                    logger.Debug("Attempting to perform first CheckForUpdate");
                    await CheckForUpdate();
                }



                // *****************************************************************
                string clientId;
                string yaml;
                if( RunningOnEC2 )
                {
                    logger.Debug("Attempting to check Ec2MetaData");
                    clientId = EC2InstanceMetadata.InstanceId;
                    yaml = EC2InstanceMetadata.UserData;
                }
                else
                    throw new InvalidOperationException( "Application is blank and RunningOnEC2 is false. AppConfig must be configured manually or from Ec2MetaData." );


                if( !string.IsNullOrWhiteSpace(clientId) )
                    ClientId = clientId;


                if( string.IsNullOrWhiteSpace(yaml) )
                    throw new InvalidOperationException("Ec2MetaData.UserData did not contain the required AppConfig settings");

                logger.LogYaml( "Settings YAML", yaml);



                // *****************************************************************
                logger.Debug("Attempting to parse yaml into Settings");
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();

                var settings = deserializer.Deserialize<Settings>(yaml);



                // *****************************************************************
                logger.Debug("Attempting to update settings");
                Application = settings.Application;
                
                if( !string.IsNullOrWhiteSpace(settings.Environment))
                    Environment = settings.Environment;

                if( !string.IsNullOrWhiteSpace(settings.Configuration) )
                    Configuration = settings.Configuration;


                // *****************************************************************
                logger.Debug("Attempting to perform first CheckForUpdate");
                await CheckForUpdate();


            }
            catch ( Exception cause )
            {
                logger.Error( cause, "Start failed" );
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected override async Task<bool> CheckForUpdate()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to build GetConfiguration request");
                var request = new GetConfigurationRequest
                {
                    Application   = Application,
                    Environment   = Environment,
                    Configuration = Configuration,
                    ClientId      = ClientId
                };

                logger.Inspect(nameof(ClientConfigurationVersion), ClientConfigurationVersion);
                if (!string.IsNullOrWhiteSpace(ClientConfigurationVersion))
                    request.ClientConfigurationVersion = ClientConfigurationVersion;



                // *****************************************************************
                logger.Debug("Attempting to execute request");
                var response = await Client.GetConfigurationAsync(request);

                if (response.Content == null || response.Content.Length == 0)
                    return false;



                // *****************************************************************
                logger.Debug("Attempting to process new Mission version");

                Source = new MemoryStream();
                await response.Content.CopyToAsync(Source);

                Source.Seek(0, SeekOrigin.Begin);



                // *****************************************************************
                logger.Debug("Attempting to update Mission version");
                ClientConfigurationVersion = response.ConfigurationVersion;
                logger.Inspect(nameof(ClientConfigurationVersion), ClientConfigurationVersion);



                // *****************************************************************
                logger.InfoFormat( "New version ({0}) acquired for Application: ({1}) Environment: ({2}) Configuration: ({3}).", ClientConfigurationVersion, Application, Environment, Configuration );
                return true;


            }
            catch (Exception cause)
            {
                var ctx = new {Application, Environment, Configuration};
                logger.ErrorWithContext( cause, ctx, "CheckForUpdate failed." );
                throw;
            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        private string ClientConfigurationVersion { get; set; }
        private MemoryStream Source { get; set; } = new MemoryStream();


        public Task<Stream> GetSource()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                if( Source.Length == 0 )
                {
                    logger.Error("Invalid state. Source is empty. Make sure AppConfigPlanSource was started successfully.");
                    throw new InvalidOperationException( "Invalid state. Source is empty" );
                }


                var stream = new MemoryStream(Source.ToArray());

                return Task.FromResult((Stream)stream);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Amazon.Util;
using Fabrica.One.Plan;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fabrica.One.Orchestrator.Aws.Plan
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



        public AppConfigPlanSource( IAmazonAppConfigData client )
        {
            Client = client;
        }

        private IAmazonAppConfigData Client { get; }


        public bool UseInstanceMetadata { get; set; } = true;

        public string Application { get; set; } = "";
        public string Environment { get; set; } = "";
        public string Configuration { get; set; } = "";
        public string ClientId { get; set; } = ShortGuid.NewGuid();


        private string ConfigurationToken { get; set; }
        private MemoryStream Source { get; set; } = new MemoryStream();


        public Task Start()
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
                if (!string.IsNullOrWhiteSpace(Application))
                    return Task.CompletedTask;



                // *****************************************************************
                string clientId;
                string yaml;
                if( UseInstanceMetadata )
                {
                    logger.Debug("Attempting to check Ec2MetaData");
                    clientId = EC2InstanceMetadata.InstanceId;
                    yaml = EC2InstanceMetadata.UserData;
                }
                else
                    throw new InvalidOperationException( "Application is blank and UseInstanceMetadata is false. AppConfig must be configured manually or from Ec2MetaData." );


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


                return Task.CompletedTask;


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

                logger.Inspect(nameof(ConfigurationToken), ConfigurationToken);


                if( string.IsNullOrWhiteSpace(ConfigurationToken) )
                {

                    // *****************************************************************
                    logger.Debug("Attempting to call StartConfigurationSession");
                    var req = new StartConfigurationSessionRequest
                    {
                        ApplicationIdentifier                = Application,
                        ConfigurationProfileIdentifier       = Configuration,
                        EnvironmentIdentifier                = Environment,
                        RequiredMinimumPollIntervalInSeconds = 15
                    };

                    logger.LogObject(nameof(req), req);

                    var res = await Client.StartConfigurationSessionAsync(req);
                    logger.LogObject(nameof(res), res);
                    if (res.HttpStatusCode != HttpStatusCode.Created)
                        throw new Exception("StartConfigurationSession Response HTTP Status Code does not indicate success");

                    ConfigurationToken = res.InitialConfigurationToken;

                }


                // *****************************************************************
                logger.Debug("Attempting to build GetConfiguration request");
                var request = new GetLatestConfigurationRequest()
                {
                    ConfigurationToken = ConfigurationToken
                };


                // *****************************************************************
                logger.Debug("Attempting to execute request");
                var response = await Client.GetLatestConfigurationAsync(request);

                if (response.Configuration == null || response.Configuration.Length == 0)
                    return false;



                // *****************************************************************
                logger.Debug("Attempting to process new Mission version");

                Source = new MemoryStream();
                await response.Configuration.CopyToAsync(Source);

                Source.Seek(0, SeekOrigin.Begin);



                // *****************************************************************
                logger.Debug("Attempting to update Mission version");
                ConfigurationToken = response.NextPollConfigurationToken;
                logger.Inspect(nameof(ConfigurationToken), ConfigurationToken);



                // *****************************************************************
                logger.InfoFormat( "New version ({0}) acquired for Application: ({1}) Environment: ({2}) Configuration: ({3}).", ConfigurationToken, Application, Environment, Configuration );
                return true;


            }
            catch (Exception cause)
            {
                var ctx = new {Application, Environment, Configuration};
                logger.ErrorWithContext( cause, ctx, "CheckForUpdate failed." );
                return false;
            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public bool IsEmpty()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var empty = Source == null || !Source.CanRead || Source.Length == 0;

                logger.Inspect(nameof(empty), empty);


                return empty;


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

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

        public Task Reload()
        {
            ConfigurationToken = null;
            return Task.CompletedTask;
        }


    }


}

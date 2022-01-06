using Amazon;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;

namespace Fabrica.Aws.Configuration.Secrets
{
    public class SecretsConfigurationSource : IConfigurationSource
    {

        public string SecretsKeyId { get; set; } = "";
        public string Region { get; set; } = "";
        public string ProfileName { get; set; } = "";
        public bool RunningOnEc2 { get; set; } = true;


        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject("this", this);

                var endpoint = RegionEndpoint.GetBySystemName(Region);

                var provider = new SecretsConfigurationProvider(SecretsKeyId, endpoint, RunningOnEc2, ProfileName);

                return provider;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

    }


}
using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Fabrica.Watch;
using Newtonsoft.Json;

namespace Fabrica.Aws.Secrets
{

    public static class AwsSecretsHelper
    {


        public static async Task PopulateWithSecrets( object target, string secretId, bool runningOnEc2, string profileName, string regionName )
        {

            var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AwsSecretsHelper));

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to check if running on EC2");
                AmazonSecretsManagerClient client;
                if (runningOnEc2)
                {
                    client = new AmazonSecretsManagerClient();
                }
                else
                {

                    var sharedFile = new SharedCredentialsFile();
                    if (!(sharedFile.TryGetProfile(profileName, out var profile) &&
                          AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials)))
                        throw new Exception($"Local profile {profile} could not be loaded");

                    var endpoint = RegionEndpoint.GetBySystemName(regionName);
                    client = new AmazonSecretsManagerClient(credentials, endpoint);

                }



                // *****************************************************************
                logger.Debug("Attempting to create AWS Secrets Managet Client");
                using (client)
                {
                    var request = new GetSecretValueRequest
                    {
                        SecretId = secretId
                    };



                    // *****************************************************************
                    logger.Debug("Attempting to get secrets YAML");
                    var response = await client.GetSecretValueAsync(request);

                    var json = response.SecretString;

                    logger.Inspect(nameof(json.Length), json.Length);



                    // *****************************************************************
                    logger.Debug("Attempting to parser JSON secrets into Confgiuration Data");
                    JsonConvert.PopulateObject(json, target);


                }


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


    }

}

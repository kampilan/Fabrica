
// ReSharper disable UnusedMember.Global

using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Fabrica.Watch;
using Newtonsoft.Json;

namespace Fabrica.Aws.Secrets;

public static class AwsSecretsHelper
{


    public static async Task PopulateWithSecrets(object target, string secretId, string profileName = "", bool runningOnEc2 = true)
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AwsSecretsHelper));


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

            client = new AmazonSecretsManagerClient(credentials);

        }



        // *****************************************************************
        logger.Debug("Attempting to create AWS Secrets Manager Client");
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
            logger.Debug("Attempting to parser JSON secrets into Configuration Data");
            JsonConvert.PopulateObject(json, target);


        }


    }


}
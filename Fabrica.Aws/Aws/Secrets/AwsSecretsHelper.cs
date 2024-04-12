
// ReSharper disable UnusedMember.Global

using System.Text.Json;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Fabrica.Watch;

namespace Fabrica.Aws.Secrets;

public static class AwsSecretsHelper
{


    public static async Task<T?> PopulateWithSecrets<T>(string secretId, string profileName = "") where T: class
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AwsSecretsHelper));


        logger.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to check if should use local credentials");
        AmazonSecretsManagerClient client;
        if( !string.IsNullOrWhiteSpace(profileName) )
        {

            var sharedFile = new SharedCredentialsFile();
            if( !(sharedFile.TryGetProfile(profileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials)) )
                throw new Exception($"Local profile {profile} could not be loaded");

            var ep = profile.Region;

            client = new AmazonSecretsManagerClient(credentials, ep);

        }
        else
        {
            client = new AmazonSecretsManagerClient();
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
            logger.Debug("Attempting to get secrets JSON");
            var response = await client.GetSecretValueAsync(request);

            var json = response.SecretString;

            logger.Inspect(nameof(json.Length), json.Length);



            // *****************************************************************
            logger.Debug("Attempting to parse JSON secrets into Configuration Data");
            var target = JsonSerializer.Deserialize<T>(json);

            return target;


        }


    }


}
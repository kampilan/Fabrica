
// ReSharper disable UnusedMember.Global

using System.Text.Json;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Fabrica.Aws;

public static class AwsSecretsHelper
{


    public static async Task<T?> PopulateWithSecrets<T>(string secretId, string profileName = "") where T: class
    {


        // *****************************************************************
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
        using (client)
        {

            var request = new GetSecretValueRequest
            {
                SecretId = secretId
            };



            // *****************************************************************
            var response = await client.GetSecretValueAsync(request);

            var json = response.SecretString;



            // *****************************************************************
            var target = JsonSerializer.Deserialize<T>(json);

            return target;


        }


    }


}
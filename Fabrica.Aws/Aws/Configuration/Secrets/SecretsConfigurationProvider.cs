using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Fabrica.Utilities.Threading;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fabrica.Aws.Configuration.Secrets;

public class SecretsConfigurationProvider: ConfigurationProvider
{

    public SecretsConfigurationProvider( string secretKeyId, RegionEndpoint endpoint, bool runningOnEc2 = true, string profileName="" )
    {

        SecretKeyId  = secretKeyId;
        Endpoint     = endpoint;
        RunningOnEc2 = runningOnEc2;
        ProfileName  = profileName;

    }


    private string SecretKeyId { get; }
    private RegionEndpoint Endpoint { get; }
    private bool RunningOnEc2 { get; }
    private string ProfileName { get; }


    public override void Load()
    {

        AsyncPump.Run(async () => await _loadSecrets());

    }

    private async Task _loadSecrets()
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(SecretKeyId), SecretKeyId);
        logger.Inspect(nameof(Endpoint), Endpoint);
        logger.Inspect(nameof(ProfileName), ProfileName);


        // *****************************************************************
        logger.Debug("Attempting to check if running on EC2");
        AWSCredentials credentials;
        if( RunningOnEc2 )
        {
            logger.Debug("Attempting to build instance profile credentials");
            credentials = new InstanceProfileAWSCredentials(ProfileName);
        }
        else
        {

            var sharedFile = new SharedCredentialsFile();
            if (!(sharedFile.TryGetProfile(ProfileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out credentials)))
                throw new Exception($"Local profile {profile} could not be loaded");

        }


        // *****************************************************************
        logger.Debug("Attempting to create AWS Secrets Managet Client");
        using var client = new AmazonSecretsManagerClient(credentials, Endpoint);

        var request = new GetSecretValueRequest
        {
            SecretId = SecretKeyId
        };



        // *****************************************************************
        logger.Debug("Attempting to get secrets YAML");
        var response = await client.GetSecretValueAsync(request);

        var yaml = response.SecretString;

        logger.Inspect(nameof(yaml.Length), yaml.Length);



        // *****************************************************************
        logger.Debug("Attempting to parse yaml into Dictionary");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();


        // *****************************************************************
        logger.Debug("Attempting to parser YAMP secrets into Confgiuration Data");
        Data = deserializer.Deserialize<Dictionary<string,string>>(yaml);


    }



}
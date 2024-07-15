using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SecurityToken;
using Autofac;

namespace Fabrica.Aws;

public static class AutofacExtensions
{

    public static ContainerBuilder UseAws(this ContainerBuilder builder, string profileName)
    {

        if (string.IsNullOrWhiteSpace(profileName))
            return builder;

        var sharedFile = new SharedCredentialsFile();
        if (!(sharedFile.TryGetProfile(profileName, out var profile) &&
              AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials)))
            throw new Exception($"Local profile {profile} could not be loaded");


        AWSConfigs.AWSRegion = profile.Region.SystemName;


        builder.Register(_ => credentials)
            .As<AWSCredentials>()
            .SingleInstance();

        return builder;

    }


    public static ContainerBuilder AddS3Client(this ContainerBuilder builder, string regionName )
    {

        builder.Register(c =>
            {

                RegionEndpoint? region = null;
                if( !string.IsNullOrWhiteSpace(regionName) )
                    region = RegionEndpoint.GetBySystemName(regionName);

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null && region is not null)
                    return new AmazonS3Client(credentials, region);

                if (region is not null)
                    return new AmazonS3Client(region);

                return new AmazonS3Client();


            })
            .As<IAmazonS3>()
            .SingleInstance();


        return builder;

    }


    public static ContainerBuilder AddS3Client(this ContainerBuilder builder )
    {

        builder.Register(c =>
        {

            var credentials = c.ResolveOptional<AWSCredentials>();

            if( credentials is not null )
                return new AmazonS3Client(credentials);
           
            
            return new AmazonS3Client();


        })
        .As<IAmazonS3>()
        .SingleInstance();


        return builder;

    }


    public static ContainerBuilder AddStsClient(this ContainerBuilder builder, string regionName)
    {

        builder.Register(c =>
            {

                RegionEndpoint? region = null;
                if (!string.IsNullOrWhiteSpace(regionName))
                    region = RegionEndpoint.GetBySystemName(regionName);

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null && region is not null)
                    return new AmazonSecurityTokenServiceClient(credentials, region);

                if (region is not null)
                    return new AmazonSecurityTokenServiceClient(region);

                return new AmazonSecurityTokenServiceClient();


            })
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        return builder;

    }


    public static ContainerBuilder AddStsClient(this ContainerBuilder builder)
    {

        builder.Register(c =>
            {

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null)
                    return new AmazonSecurityTokenServiceClient(credentials);


                return new AmazonSecurityTokenServiceClient();


            })
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        return builder;

    }

    public static ContainerBuilder AddStsConfiguration(this ContainerBuilder builder, string roleArn, string policy)
    {

        builder.Register( _ =>
            {
                var comp = new StsConfiguration
                {
                    RoleArn = roleArn,
                    Policy = policy
                };

                return comp;
            })
            .AsSelf()
            .SingleInstance();

        return builder;

    }




}
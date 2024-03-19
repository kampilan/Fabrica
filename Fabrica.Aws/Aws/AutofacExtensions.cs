/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global

using Amazon;
using Amazon.AppConfig;
using Amazon.AppConfigData;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecurityToken;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using Autofac;
using Fabrica.Aws.Repository;
using Fabrica.Aws.Storage;
using Fabrica.Repository;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Repository;
using Fabrica.Utilities.Storage;
using Fabrica.Watch;

namespace Fabrica.Aws;

public static class AutofacExtensions
{


    public static ContainerBuilder UseAws(this ContainerBuilder builder, string profileName )
    {


        using var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AutofacExtensions));


        logger.EnterScope(nameof(UseAws));

        if (string.IsNullOrWhiteSpace(profileName))
            return UseAws(builder);


        logger.Debug("Attempting to build local profile credentials");
            var sharedFile = new SharedCredentialsFile();
            if (!(sharedFile.TryGetProfile(profileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials)))
                throw new Exception($"Local profile {profile} could not be loaded");

            logger.Inspect(nameof(profile.Region), profile.Region.SystemName);

            AWSConfigs.AWSRegion = profile.Region.SystemName;



        builder.Register(c => credentials)
            .As<AWSCredentials>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonS3Client(credentials))
            .As<IAmazonS3>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSecurityTokenServiceClient(credentials))
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSQSClient(credentials))
            .As<IAmazonSQS>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSimpleNotificationServiceClient(credentials))
            .As<IAmazonSimpleNotificationService>()
            .SingleInstance();



        builder.Register(c => new AmazonSimpleSystemsManagementClient(credentials))
            .As<IAmazonSimpleSystemsManagement>()
            .SingleInstance();



        // ******************************************************************
        builder.Register(c => new AmazonAppConfigClient(credentials))
            .As<IAmazonAppConfig>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonAppConfigDataClient(credentials))
            .As<IAmazonAppConfigData>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSecretsManagerClient(credentials))
            .As<IAmazonSecretsManager>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonDynamoDBClient(credentials))
            .As<IAmazonDynamoDB>()
            .SingleInstance();

        // ******************************************************************
        builder.Register(c => new DynamoDBContext(c.Resolve<IAmazonDynamoDB>()))
            .As<IDynamoDBContext>()
            .InstancePerDependency();



        // ******************************************************************
        return builder;


    }



    public static ContainerBuilder UseAws(this ContainerBuilder builder)
    {


        using var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AutofacExtensions));


        logger.EnterScope(nameof(UseAws));


        // ******************************************************************
        builder.Register(c => new AmazonS3Client())
            .As<IAmazonS3>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSecurityTokenServiceClient())
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSQSClient())
            .As<IAmazonSQS>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSimpleNotificationServiceClient())
            .As<IAmazonSimpleNotificationService>()
            .SingleInstance();



        builder.Register(c => new AmazonSimpleSystemsManagementClient())
            .As<IAmazonSimpleSystemsManagement>()
            .SingleInstance();



        // ******************************************************************
        builder.Register(c => new AmazonAppConfigClient())
            .As<IAmazonAppConfig>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonAppConfigDataClient())
            .As<IAmazonAppConfigData>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonSecretsManagerClient())
            .As<IAmazonSecretsManager>()
            .SingleInstance();


        // ******************************************************************
        builder.Register(c => new AmazonDynamoDBClient())
            .As<IAmazonDynamoDB>()
            .SingleInstance();

        // ******************************************************************
        builder.Register(c => new DynamoDBContext(c.Resolve<IAmazonDynamoDB>()))
            .As<IDynamoDBContext>()
            .InstancePerDependency();



        // ******************************************************************
        return builder;


    }




    public static ContainerBuilder AddStorage(this ContainerBuilder builder)
    {

        builder.Register(c =>
            {
                var corr = c.Resolve<ICorrelation>();
                var client = c.Resolve<IAmazonS3>();
                var comp = new StorageComponent(corr, client);
                return comp;
            })
            .As<IStorageProvider>()
            .As<IRemoteStorageProvider>()
            .SingleInstance()
            .AutoActivate();


        return builder;

    }


    public static ContainerBuilder AddRepositoryProvider(this ContainerBuilder builder, string repository)
    {

        builder.Register(c =>
            {

                var corr = c.Resolve<ICorrelation>();
                var s3 = c.Resolve<IAmazonS3>();

                var comp = new S3RepositoryProvider(corr, s3, repository);

                return comp;

            })
            .As<IRepositoryProvider>()
            .SingleInstance();

        return builder;

    }

    public static ContainerBuilder AddRepositoryProvider(this ContainerBuilder builder, IRepositoryConfiguration config)
    {

        builder.Register(c =>
            {

                var corr = c.Resolve<ICorrelation>();
                var s3 = c.Resolve<IAmazonS3>();

                var comp = new S3RepositoryProvider(corr, s3, config.RepositoryContainer);

                return comp;

            })
            .As<IRepositoryProvider>()
            .SingleInstance();

        return builder;

    }

    public static ContainerBuilder AddStsConfiguration(this ContainerBuilder builder, string roleArn, string policy )
    {

        builder.Register(c =>
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
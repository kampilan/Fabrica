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

using System;
using Amazon;
using Amazon.AppConfig;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecurityToken;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using Autofac;
using Fabrica.Aws.Repository;
using Fabrica.Aws.Storage;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Repository;
using Fabrica.Utilities.Storage;
using Fabrica.Watch;
using JetBrains.Annotations;
// ReSharper disable UnusedParameter.Local

namespace Fabrica.Aws
{

    public static class AutofacExtensions
    {


        public static ContainerBuilder UseAws(this ContainerBuilder builder, [NotNull] IAwsCredentialModule module )
        {

            if (module == null) throw new ArgumentNullException(nameof(module));


            var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AutofacExtensions));

            try
            {

                logger.EnterScope( nameof(UseAws) );


                logger.LogObject(nameof(module), module);


                // *****************************************************************
                logger.Debug("Attempting to check for AccessKey");
                if (!string.IsNullOrWhiteSpace(module.AccessKey))
                    return builder.UseAws( module.RegionName, module.AccessKey, module.SecretKey );



                // *****************************************************************
                logger.Debug("Attempting to check for Profile");
                if (!string.IsNullOrWhiteSpace(module.Profile))
                    return builder.UseAws( module.RegionName, module.Profile, module.RunningOnEC2 );



                // *****************************************************************
                logger.Debug("Attempting to use AWS with automatic credentials");
                if( !string.IsNullOrWhiteSpace(module.RegionName) )
                    return builder.UseAws( module.RegionName );


                // *****************************************************************
                return builder.UseAws();


            }
            finally
            {
                logger.LeaveScope( nameof(UseAws) );
            }



        }

        public static ContainerBuilder UseAws( this ContainerBuilder builder, string regionName="" )
        {


            var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AutofacExtensions));

            try
            {

                logger.EnterScope(nameof(UseAws));


                // ******************************************************************
                if( !string.IsNullOrWhiteSpace(regionName))
                    _addServices( builder, regionName );
                else
                    _addServices( builder );


                // ******************************************************************
                return builder;


            }
            finally
            {
                logger.LeaveScope(nameof(UseAws));
            }


        }

        public static ContainerBuilder UseAws(this ContainerBuilder builder, [NotNull] string regionName, [NotNull] string accessKey, [NotNull] string secretKey )
        {


            if (string.IsNullOrWhiteSpace(regionName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(regionName));

            if (string.IsNullOrWhiteSpace(accessKey))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessKey));

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(secretKey));


            var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AutofacExtensions));

            try
            {

                logger.EnterScope(nameof(UseAws));



                // *****************************************************************
                logger.Debug("Attempting to build Basic credentials");
                var credentials = new BasicAWSCredentials(accessKey, secretKey);



                // *****************************************************************
                logger.Debug("Attempting to add services");
                _addServices(builder, credentials, regionName);



                // ******************************************************************
                return builder;


            }
            finally
            {
                logger.LeaveScope(nameof(UseAws));
            }


        }

        public static ContainerBuilder UseAws(this ContainerBuilder builder, [NotNull] string regionName, [NotNull] string profileName, bool runningOnEc2=false )
        {

            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(profileName));


            var logger = WatchFactoryLocator.Factory.GetLogger(typeof(AutofacExtensions));

            try
            {

                logger.EnterScope(nameof(UseAws));


                // *****************************************************************
                logger.Debug("Attempting to check if running on EC2");
                AWSCredentials credentials;
                if( runningOnEc2 )
                {
                    logger.Debug("Attempting to build instance profile credentials");
                    credentials = new InstanceProfileAWSCredentials(profileName);
                }
                else
                {

                    var sharedFile = new SharedCredentialsFile();
                    if( !(sharedFile.TryGetProfile(profileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out credentials)) )
                        throw new Exception($"Local profile {profile} could not be loaded");

                }


                // ******************************************************************
                _addServices(builder, credentials, regionName );



                // ******************************************************************
                return builder;


            }
            finally
            {
                logger.LeaveScope(nameof(UseAws));
            }


        }


        private static void _addServices(ContainerBuilder builder)
        {



            // ******************************************************************
            builder.Register(c => new AmazonS3Client())
                .As<IAmazonS3>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSecurityTokenServiceClient())
                .As<IAmazonSecurityTokenService>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSQSClient())
                .As<IAmazonSQS>()
                .SingleInstance()
                .AutoActivate();


            builder.Register(c => new AmazonSimpleSystemsManagementClient())
                .As<IAmazonSimpleSystemsManagement>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonAppConfigClient())
                .As<IAmazonAppConfig>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSecretsManagerClient())
                .As<IAmazonSecretsManager>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonDynamoDBClient())
                .As<IAmazonDynamoDB>()
                .SingleInstance()
                .AutoActivate();

            // ******************************************************************
            builder.Register(c => new DynamoDBContext(c.Resolve<IAmazonDynamoDB>()))
                .As<IDynamoDBContext>()
                .InstancePerDependency();


        }


        private static void _addServices(ContainerBuilder builder, string regionName)
        {


            // ******************************************************************
            var endpoint = RegionEndpoint.GetBySystemName(regionName);


            // ******************************************************************
            builder.Register(c => new AmazonS3Client(endpoint))
                .As<IAmazonS3>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSecurityTokenServiceClient(endpoint))
                .As<IAmazonSecurityTokenService>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSQSClient(endpoint))
                .As<IAmazonSQS>()
                .SingleInstance()
                .AutoActivate();


            builder.Register(c => new AmazonSimpleSystemsManagementClient(endpoint))
                .As<IAmazonSimpleSystemsManagement>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonAppConfigClient(endpoint))
                .As<IAmazonAppConfig>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSecretsManagerClient(endpoint))
                .As<IAmazonSecretsManager>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonDynamoDBClient(endpoint))
                .As<IAmazonDynamoDB>()
                .SingleInstance()
                .AutoActivate();

            // ******************************************************************
            builder.Register(c => new DynamoDBContext(c.Resolve<IAmazonDynamoDB>()))
                .As<IDynamoDBContext>()
                .InstancePerDependency();


        }



        private static void _addServices( ContainerBuilder builder,  AWSCredentials credentials, string regionName )
        {


            // ******************************************************************
            var endpoint = RegionEndpoint.GetBySystemName(regionName);


            // ******************************************************************
            builder.Register(c => new AmazonS3Client(credentials, endpoint))
                .As<IAmazonS3>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSecurityTokenServiceClient(credentials, endpoint))
                .As<IAmazonSecurityTokenService>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSQSClient(credentials, endpoint))
                .As<IAmazonSQS>()
                .SingleInstance()
                .AutoActivate();


            builder.Register(c => new AmazonSimpleSystemsManagementClient( credentials, endpoint))
                .As<IAmazonSimpleSystemsManagement>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonAppConfigClient( credentials, endpoint) )
                .As<IAmazonAppConfig>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonSecretsManagerClient( credentials, endpoint))
                .As<IAmazonSecretsManager>()
                .SingleInstance()
                .AutoActivate();


            // ******************************************************************
            builder.Register(c => new AmazonDynamoDBClient( credentials, endpoint))
                .As<IAmazonDynamoDB>()
                .SingleInstance()
                .AutoActivate();

            // ******************************************************************
            builder.Register(c => new DynamoDBContext(c.Resolve<IAmazonDynamoDB>()))
                .As<IDynamoDBContext>()
                .InstancePerDependency();


        }


        public static ContainerBuilder AddStorage(this ContainerBuilder builder)
        {

            builder.Register(c =>
                {
                    var corr   = c.Resolve<ICorrelation>();
                    var client = c.Resolve<IAmazonS3>();
                    var comp   = new StorageComponent( corr, client );
                    return comp;
                })
                .As<IStorageProvider>()
                .As<IRemoteStorageProvider>()
                .SingleInstance()
                .AutoActivate();


            return builder;

        }

        public static ContainerBuilder AddUrlProvider(this ContainerBuilder builder, string permanent, string transient, string resource)
        {

            builder.Register(c =>
                {

                    var s3 = c.Resolve<IAmazonS3>();

                    var comp = new S3RepositoryUrlProvider(s3, permanent, transient, resource);

                    return comp;

                })
                .As<IRepositoryUrlProvider>()
                .SingleInstance();

            return builder;

        }

        public static ContainerBuilder AddUrlProvider(this ContainerBuilder builder, IRepositoryConfiguration config )
        {

            builder.Register(c =>
                {

                    var s3 = c.Resolve<IAmazonS3>();

                    var comp = new S3RepositoryUrlProvider(s3, config.PermanentContainer, config.TransientContainer, config.ResourceContainer);

                    return comp;

                })
                .As<IRepositoryUrlProvider>()
                .SingleInstance();

            return builder;

        }


    }



}

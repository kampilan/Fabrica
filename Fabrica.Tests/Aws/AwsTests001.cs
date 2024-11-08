﻿using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Autofac;
using Fabrica.Aws;
using Fabrica.Watch;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.Tests.Aws;

[TestFixture]
public class AwsTests001
{

    [Test]
    public async Task Test_0850_0100_Should_Return_AwsSecrets()
    {

        var model = await AwsSecretsHelper.PopulateWithSecrets<SecretsModel>( "kampilan-local", "kampilan" );

        ClassicAssert.IsNotEmpty(model.OriginDbPassword);

    }

    [Test]
    public async Task Test_0850_Should_Find_SQS_Queue()
    {
/*
        var builder = new ContainerBuilder();
        builder.UseAws("kampilan");

        var container = builder.Build();
        var scope = container.BeginLifetimeScope();

        var client = scope.Resolve<IAmazonSQS>();

        var request = new GetQueueUrlRequest
        {
            QueueName = "fabrica-work"
        };

        var response = await client.GetQueueUrlAsync(request);

        ClassicAssert.IsNotNull(response);
*/
    }


    [Test]
    public async Task Test_0850_0200_Should_Get_Sts_Token()
    {

        var builder = new ContainerBuilder();
        builder.UseAws("kampilan");

        var container = builder.Build();
        var scope = container.BeginLifetimeScope();

        var client = scope.Resolve<IAmazonSecurityTokenService>();

        var set = await client.CreateCredentialSet("arn:aws:iam::523329725044:role/client-serene", "Moring0001",  duration:TimeSpan.FromHours(8) );

        ClassicAssert.NotNull(set);

        ClassicAssert.IsNotEmpty(set.AccessKey);
        ClassicAssert.IsNotEmpty(set.SecretKey);
        ClassicAssert.IsNotEmpty(set.SessionToken);

        ClassicAssert.IsTrue( set.Expiration > DateTime.Now );

        var creds = new SessionAWSCredentials(set.AccessKey, set.SecretKey, set.SessionToken);

        var s3 = new AmazonS3Client(creds, RegionEndpoint.USEast2);

        var request = new GetObjectMetadataRequest
        {
            BucketName = "after-serene-permanent",
            Key = "indexes/ResourceAvailability.bin"
        };

        var response = await s3.GetObjectMetadataAsync(request);

        ClassicAssert.NotNull(response);
        ClassicAssert.IsTrue(response.HttpStatusCode == HttpStatusCode.OK);


    }


    [Test]
    public async Task Test_0850_0300_Should_Get_Creds_Automatically()
    {


        var builder = new ContainerBuilder();
//        builder.UseAws();

        var container = builder.Build();
        var scope = container.BeginLifetimeScope();


        var client = scope.Resolve<IAmazonS3>();

        var request = new GetObjectRequest
        {
            BucketName = "pondhawk-appliance-repository",
            Key = "missions/fakeproduction-production-mission-plan.json"
        };

        var response = await client.GetObjectAsync(request);

        ClassicAssert.IsTrue(response.HttpStatusCode == HttpStatusCode.OK);


    }

    [Test]
    public async Task Test_0850_0300_Should_Get_Creds_ByProfile()
    {


        var builder = new ContainerBuilder();
        builder.UseAws("kampilan");

        var container = builder.Build();
        var scope = container.BeginLifetimeScope();


        var client = scope.Resolve<IAmazonS3>();

        var request = new GetObjectRequest
        {
            BucketName = "pondhawk-appliance-repository",
            Key = "missions/fakeproduction-production-mission-plan.json"
        };

        var response = await client.GetObjectAsync(request);

        ClassicAssert.IsTrue(response.HttpStatusCode == HttpStatusCode.OK);


    }




}

public class SecretsModel
{

    [JsonPropertyName("origin-db-user-name")]
    public string OriginDbUserName { get; set; } = "";
    [Sensitive]
    [JsonPropertyName("origin-db-password")]
    public string OriginDbPassword { get; set; } = "";

    [JsonPropertyName("replica-db-user-name")]
    public string ReplicaDbUserName { get; set; } = "";
    [Sensitive]
    [JsonPropertyName("replica-db-password")]
    public string ReplicaDbPassword { get; set; } = "";

}




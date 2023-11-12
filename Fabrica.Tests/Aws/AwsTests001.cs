using System;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SQS;
using Amazon.SQS.Model;
using Autofac;
using Fabrica.Aws;
using Fabrica.Aws.Secrets;
using Fabrica.Watch;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Fabrica.Tests.Aws;

[TestFixture]
public class AwsTests001
{

    [Test]
    public async Task Test_0850_0100_Should_Return_AwsSecrets()
    {

        var model = new SecretsModel();
        await AwsSecretsHelper.PopulateWithSecrets(model, "kampilan-local", "kampilan", true );

        Assert.IsNotEmpty(model.OriginDbPassword);

    }

    [Test]
    public async Task Test_0850_Should_Find_SQS_Queue()
    {

        var builder = new ContainerBuilder();
        builder.UseAws("kampilan", true);

        var container = builder.Build();
        var scope = container.BeginLifetimeScope();

        var client = scope.Resolve<IAmazonSQS>();

        var request = new GetQueueUrlRequest
        {
            QueueName = "fabrica-work"
        };

        var response = await client.GetQueueUrlAsync(request);

        Assert.IsNotNull(response);

    }


    [Test]
    public async Task Test_0850_0200_Should_Get_Sts_Token()
    {

        var builder = new ContainerBuilder();
        builder.UseAws("kampilan", true);

        var container = builder.Build();
        var scope = container.BeginLifetimeScope();

        var client = scope.Resolve<IAmazonSecurityTokenService>();

        var set = await client.CreateCredentialSet("arn:aws:iam::523329725044:role/client-serene", "Moring0001");

        Assert.IsNotNull(set);

        Assert.IsNotEmpty(set.AccessKey);
        Assert.IsNotEmpty(set.SecretKey);
        Assert.IsNotEmpty(set.SessionToken);

        Assert.IsTrue( set.Expiration > DateTime.Now );

        var creds = new SessionAWSCredentials(set.AccessKey, set.SecretKey, set.SessionToken);

        var s3 = new AmazonS3Client(creds, RegionEndpoint.USEast2);

        var request = new GetObjectMetadataRequest
        {
            BucketName = "after-serene-permanent",
            Key = "indexes/ResourceAvailability.bin"
        };

        var response = await s3.GetObjectMetadataAsync(request);

        Assert.NotNull(response);
        Assert.IsTrue(response.HttpStatusCode == HttpStatusCode.OK);


    }






}

public class SecretsModel
{

    [JsonProperty("origin-db-user-name")]
    public string OriginDbUserName { get; set; } = "";
    [Sensitive]
    [JsonProperty("origin-db-password")]
    public string OriginDbPassword { get; set; } = "";

    [JsonProperty("replica-db-user-name")]
    public string ReplicaDbUserName { get; set; } = "";
    [Sensitive]
    [JsonProperty("replica-db-password")]
    public string ReplicaDbPassword { get; set; } = "";

}




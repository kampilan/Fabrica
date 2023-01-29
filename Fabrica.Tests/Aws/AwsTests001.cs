using System.Threading.Tasks;
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




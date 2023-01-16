using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Make.Sdk;
using NUnit.Framework;

namespace Fabrica.Tests.Make;

[TestFixture]
public class MakeTest001
{

    [OneTimeSetUp]
    public void Setup()
    {

        var builder = new ContainerBuilder();

        builder.RegisterModule<Make001Module>();

        TheContainer = builder.Build();

    }

    [OneTimeTearDown]
    public void Teardown()
    {
        TheContainer.Dispose();
    }

    private IContainer TheContainer { get; set; }


    [Test]
    public async Task Test2000_0100_Can_Call_Scenario()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var factory = scope.Resolve<IHttpClientFactory>();
        var client = factory.CreateClient("MakeApi");

        var body = new {Destination = "2142289941", Messsage = "Just a test."};

        var request = new MakeExecuteRequest( "619773", body );

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        Assert.IsTrue(true);

    }


}

public class Make001Module : Module
{
    protected override void Load(ContainerBuilder builder)
    {

        builder.AddMakeApiClient("https://us1.make.com/api/v2/", "28c584a8-d600-4174-84c3-2868e782d5a2");

    }
}    
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Make.Sdk;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.Tests.Make;

[TestFixture]
public class MakeTest001
{

    [OneTimeSetUp]
    public void Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();

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


    [Test]
    public async Task Test2000_0200_Can_Get_Scenarios()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var factory = scope.Resolve<IHttpClientFactory>();

        var res = await factory.GetScenarios(60295);

        Assert.IsNotNull(res);
        Assert.IsNotEmpty(res.Scenarios);

    }


    [Test]
    public async Task Test2000_0300_Can_Get_Hooks()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var factory = scope.Resolve<IHttpClientFactory>();

        var res = await factory.GetHooks(60295);

        Assert.IsNotNull(res);
        Assert.IsNotEmpty(res.Hooks);

    }





}

public class Make001Module : Module
{
    protected override void Load(ContainerBuilder builder)
    {

        builder.AddMakeApiClient("https://us1.make.com/api/v2/", "fe881c57-3539-4659-9bc9-5cbe5894b8c8");

    }
}    
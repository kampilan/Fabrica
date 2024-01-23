using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.Identity.Keycloak;
using Fabrica.Identity.Keycloak.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.Tests.Identity.Keycloak;

[TestFixture]
public class KeycloakProviderTests
{

    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Debug, Color.Aqua);

        maker.Build();

        var builder = new ContainerBuilder();
        builder.RegisterModule<TheModule>();


        TheContainer = builder.Build();

        await TheContainer.StartComponents();


    }

    [OneTimeTearDown]
    public void Teardown()
    {
        TheContainer.Dispose();
        WatchFactoryLocator.Factory.Stop();
    }


    private IContainer TheContainer { get; set; }

    [Test]
    public async Task Test3100_0100_Should_Return_Users_And_UserByUid()
    {


        for (int x = 0; x < 10; x++)
        {
            await using var scope = TheContainer.BeginLifetimeScope();

            var factory = scope.Resolve<IHttpClientFactory>();
            using var client = factory.CreateClient("Keycloak");

            var json = await client.GetStringAsync("users");

            Assert.IsNotNull(json);
            Assert.IsNotEmpty(json);

            var list = JsonSerializer.Deserialize<List<User>>(json);

            Assert.IsNotNull(list);
            Assert.IsNotEmpty(list);

            var user = list.First();

            Assert.IsNotNull(user);


            var json2 = await client.GetStringAsync($"users/{user.Id}");


            Assert.IsNotNull(json2);
            Assert.IsNotEmpty(json2);

            var user2 = JsonSerializer.Deserialize<User>(json2);

            Assert.IsNotNull(user2);

        }


    }


    [Test]
    public async Task Test3100_0200_Should_Create_New_User()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var provider = scope.Resolve<IIdentityProvider>();

        var request = new SyncUserRequest
        {
            Upsert = true,
            NewUsername = "wilma.moring",
            NewEmail = "wilma.l.moring@gmail.com",
            NewFirstName = "Wilma",
            NewLastName = "Moring",
            GeneratePassword = true,
        };

//        request.Groups.Add("Managers");
//        request.Groups.Add("after-tenant");
        request.Attributes["Cool"] = new[] {"Very"};


        var res = await provider.SyncUser(request);

        Assert.IsNotNull(res);

        Assert.IsNotEmpty(res.IdentityUid);
        Assert.IsFalse(res.Exists);
        Assert.IsFalse(res.Updated);
        Assert.IsTrue(res.Created);



    }


    [Test]
    public async Task Test3100_0210_Should_Create_New_User_And_Import_Bcrypt()
    {

        var hash = BCrypt.Net.BCrypt.HashPassword("GoNavyGoBears", 15);

        await using var scope = TheContainer.BeginLifetimeScope();
        var provider = scope.Resolve<IIdentityProvider>();

        var request = new SyncUserRequest
        {
            Upsert = true,
            NewUsername = "gabby.moring",
            NewEmail = "moring.gabby@gmail.com",
            NewFirstName = "Gabby",
            NewLastName = "Moring",
            HashAlgorithm = "bcrypt",
            HashIterations = 15,
            HashedPassword = hash,
        };

        //        request.Groups.Add("Managers");
        //        request.Groups.Add("after-tenant");
        request.Attributes["Cool"] = new[] { "Very" };


        var res = await provider.SyncUser(request);

        Assert.IsNotNull(res);

        Assert.IsNotEmpty(res.IdentityUid);
        Assert.IsFalse(res.Exists);
        Assert.IsFalse(res.Updated);
        Assert.IsTrue(res.Created);

    }



    [Test]
    public async Task Test3100_0220_Should_Update_User()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var provider = scope.Resolve<IIdentityProvider>();

        var request = new SyncUserRequest
        {
            CurrentUsername = "gabby.moring",
            NewEmail = "james.moring@kampilangroup.com",
            NewEnabled = false,
            MustVerifyEmail = false
        };


        var res = await provider.SyncUser(request);

        Assert.IsNotNull(res);

        Assert.IsNotEmpty(res.IdentityUid);
        Assert.IsTrue(res.Exists);
        Assert.IsTrue(res.Updated);
        Assert.IsFalse(res.Created);

    }


    [Test]
    public async Task Test3100_0230_Should_Not_Update_User()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var provider = scope.Resolve<IIdentityProvider>();

        var request = new SyncUserRequest
        {
            CurrentUsername = "gabby.moring",
            NewEmail = "me@jamesmoring.com",
            NewEnabled = false,
            MustVerifyEmail = true
        };


        var res = await provider.SyncUser(request);

        Assert.IsNotNull(res);

        Assert.IsNotEmpty(res.IdentityUid);
        Assert.IsTrue(res.Exists);
        Assert.IsFalse(res.Updated);
        Assert.IsFalse(res.Created);

    }



    [Test]
    public async Task Test3100_0300_GetAccessToken()
    {

        using( var scope = TheContainer.BeginLifetimeScope() )
        {

            var source = scope.Resolve<IAccessTokenSource>();

            var token = await source.GetToken();

            Assert.IsNotEmpty(token);

        }

    }


    [Test]
    public async Task Test3100_0300_Introspect()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {
            var factory = scope.Resolve<IHttpClientFactory>();
            var client = factory.CreateClient("Api");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri( client.BaseAddress, "api/introspection/")
            };

            var response = await client.SendAsync(request);

            Assert.IsNotNull( response );
            Assert.IsTrue( response.IsSuccessStatusCode );

            var json = await response.Content.ReadAsStringAsync();

            Assert.IsNotEmpty( json );

            var intro = JsonNode.Parse(json);

            Assert.IsNotNull(intro);


        }

    }



    [Test]
    public void Test3100_0400_ImportUser()
    {

        var hash = BCrypt.Net.BCrypt.HashPassword("Myxxxxxxxxxxxx", 13);

    }







}

public class TheModule : Module
{

    public string KeycloakMetaEndpoint { get; set; } = "https://identity.after-it-services.io/realms/master/.well-known/openid-configuration";
    public string KeycloakApiEndpoint { get; set; } = "https://identity.after-it-services.io/admin/realms/pondhawk/";

    public string PondHawkMetaEndpoint { get; set; } = "https://identity.after-it-services.io/realms/pondhawk/.well-known/openid-configuration";

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();


        builder.AddKeycloakIdentityProvider(o =>
        {

            o.GrantType = TokenApiGrantType.ClientCredential;
            o.MetaEndpoint = KeycloakMetaEndpoint;
            o.ApiEndpoint = KeycloakApiEndpoint;
            o.ClientId = "service";
            o.ClientSecret = "aQc6tEyajZ6ZpGQPxQarUgscO14ropXN";

        });

    }

}
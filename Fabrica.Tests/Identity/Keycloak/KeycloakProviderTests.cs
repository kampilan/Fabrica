using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Configuration.Yaml;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.Identity.Keycloak;
using Fabrica.Identity.Keycloak.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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


        var cb = new ConfigurationBuilder()
            .AddYamlFile("e:/locals/fabrica/tests/keycloak/pht.yml", true);
        var cfg = cb.Build();

        var module = cfg.Get<TheModule>();

        var builder = new ContainerBuilder();
        builder.RegisterModule(module);


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

            ClassicAssert.IsNotNull(json);
            ClassicAssert.IsNotEmpty(json);

            var list = JsonSerializer.Deserialize<List<User>>(json);

            ClassicAssert.IsNotNull(list);
            ClassicAssert.IsNotEmpty(list);

            var user = list.First();

            ClassicAssert.IsNotNull(user);


            var json2 = await client.GetStringAsync($"users/{user.Id}");


            ClassicAssert.IsNotNull(json2);
            ClassicAssert.IsNotEmpty(json2);

            var user2 = JsonSerializer.Deserialize<User>(json2);

            ClassicAssert.IsNotNull(user2);

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

        ClassicAssert.IsNotNull(res);

        ClassicAssert.IsNotEmpty(res.IdentityUid);
        ClassicAssert.IsFalse(res.Exists);
        ClassicAssert.IsFalse(res.Updated);
        ClassicAssert.IsTrue(res.Created);



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

        ClassicAssert.IsNotNull(res);

        ClassicAssert.IsNotEmpty(res.IdentityUid);
        ClassicAssert.IsFalse(res.Exists);
        ClassicAssert.IsFalse(res.Updated);
        ClassicAssert.IsTrue(res.Created);

    }

    [Test]
    public async Task Test3100_0210_Should_Create_New_User_And_Custom_Password()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var provider = scope.Resolve<IIdentityProvider>();

        var request = new SyncUserRequest
        {
            Upsert = true,
            CurrentUsername = "gabby.moring",
            NewPassword = "Monkey!2345"
        };

        //        request.Groups.Add("Managers");
        //        request.Groups.Add("after-tenant");
        request.Attributes["Cool"] = new[] { "Very" };


        var res = await provider.SyncUser(request);

        ClassicAssert.IsNotNull(res);

        ClassicAssert.IsNotEmpty(res.IdentityUid);
        ClassicAssert.IsFalse(res.Exists);
        ClassicAssert.IsFalse(res.Updated);
        ClassicAssert.IsTrue(res.Created);

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

        ClassicAssert.IsNotNull(res);

        ClassicAssert.IsNotEmpty(res.IdentityUid);
        ClassicAssert.IsTrue(res.Exists);
        ClassicAssert.IsTrue(res.Updated);
        ClassicAssert.IsFalse(res.Created);

    }


    [Test]
    public async Task Test3100_0221_Should_Update_Password()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var provider = scope.Resolve<IIdentityProvider>();

        var request = new SyncUserRequest
        {
            CurrentUsername = "gabby.moring",
            NewPassword = "TestTest123"
        };


        var res = await provider.SyncUser(request);

        ClassicAssert.IsNotNull(res);

        ClassicAssert.IsNotEmpty(res.IdentityUid);
        ClassicAssert.IsTrue(res.Exists);
        ClassicAssert.IsTrue(res.Updated);
        ClassicAssert.IsFalse(res.Created);

    }


    [Test]
    public async Task Test3100_0222_Should_Send_Update_Password_Email()
    {

        await using var scope = TheContainer.BeginLifetimeScope();
        var provider = scope.Resolve<IIdentityProvider>();

        var res = await provider.ExecutePasswordReset("james.moring@kampilangroup.com");


        ClassicAssert.IsTrue(res);

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

        ClassicAssert.IsNotNull(res);

        ClassicAssert.IsNotEmpty(res.IdentityUid);
        ClassicAssert.IsTrue(res.Exists);
        ClassicAssert.IsFalse(res.Updated);
        ClassicAssert.IsFalse(res.Created);

    }



    [Test]
    public async Task Test3100_0300_GetAccessToken()
    {

        using( var scope = TheContainer.BeginLifetimeScope() )
        {

            var source = scope.Resolve<IAccessTokenSource>();

            var token = await source.GetToken();

            ClassicAssert.IsNotEmpty(token);

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

            ClassicAssert.IsNotNull( response );
            ClassicAssert.IsTrue( response.IsSuccessStatusCode );

            var json = await response.Content.ReadAsStringAsync();

            ClassicAssert.IsNotEmpty( json );

            var intro = JsonNode.Parse(json);

            ClassicAssert.IsNotNull(intro);


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

    public string KeycloakMetaEndpoint { get; set; } = string.Empty;
    public string KeycloakApiEndpoint { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();


        builder.AddKeycloakIdentityProvider(o =>
        {

            o.GrantType = TokenApiGrantType.ClientCredential;
            o.MetaEndpoint = KeycloakMetaEndpoint;
            o.ApiEndpoint = KeycloakApiEndpoint;
            o.ClientId = ClientId;
            o.ClientSecret = ClientSecret;

        });

    }

}


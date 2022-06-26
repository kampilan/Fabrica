using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Fabrica.Tests.Identity.Keycloak;

[TestFixture]
public class KeycloakProviderTests
{

    [SetUp]
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


    private IContainer TheContainer { get; set; }

    [Test]
    public async Task Test3100_0100_SyncExistingUserById()
    {

        var provider = new KeycloakIdentityProvider( new Correlation(),"https://identity.after-it-services.io", "after-crm", "E3fBZg4qCYreKHq8QRLNOeZFEGulFVg8");

        var request = new SyncUserRequest
        {
            IdentityUid     = "ed390706-55df-43c8-b904-2287d5aa38f5",
            NewFirstName    = "James"
        };

        var res = await provider.SyncUser( request );

        Assert.IsNotNull(res);

        Assert.IsNotEmpty(res.IdentityUid);
        Assert.IsFalse(res.Created);

    }


    [Test]
    public async Task Test3100_0200_SyncNew()
    {

        var provider = new KeycloakIdentityProvider(new Correlation(), "https://identity.after-it-services.io", "after-crm", "E3fBZg4qCYreKHq8QRLNOeZFEGulFVg8");

        var request = new SyncUserRequest
        {
            NewUsername = "wilma.moring",
            NewEmail = "wilma.l.moring@gmail.com",
            NewFirstName = "Wilma",
            NewLastName = "Moring",
            GeneratePassword = false,
            MustUpdatePassword = true
        };

        request.Groups.Add("Managers");
        request.Groups.Add("after-tenant");
        request.Attributes["Cool"] = new[] {"Very"};


        var res = await provider.SyncUser(request);

        Assert.IsNotNull(res);

        Assert.IsNotEmpty(res.IdentityUid);
        Assert.IsTrue(res.Created);

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



}

public class TheModule : Module
{

    public string MetaEndpoint { get; set; } = "https://identity.after-it-services.io/auth/realms/after-crm/.well-known/openid-configuration";

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.AddClientCredentialGrant("Api", MetaEndpoint, "service", "IX9RYgB0iAJeTIdHS3RMbkdakJOYqrkv");

        builder.AddAccessTokenSource("Api");

        builder.Register(c =>
            {

                var sources = c.Resolve<IEnumerable<IAccessTokenSource>>();

                var comp = new AccessTokenSourceRequestHandler(sources);
                comp.Name = "Api";

                return comp;

            })
            .AsSelf()
            .InstancePerDependency();


        builder.AddHttpClient<AccessTokenSourceRequestHandler>("Api", "https://fabrica.ngrok.io/v1/");



    }

}
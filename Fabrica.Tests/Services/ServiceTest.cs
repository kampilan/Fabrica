﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Autofac;
using Autofac.Core;
using Bogus;
using Dynamitey;
using Fabrica.Mediator;
using Fabrica.Services;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using ImpromptuInterface;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using IContainer = Autofac.IContainer;

namespace Fabrica.Tests.Services;


[TestFixture]
public class ServiceTest
{

    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();


        var builder = new ContainerBuilder();

        builder.RegisterModule(new ServiceTestModule());

        TheContainer = await builder.BuildAndStart();

    }

    [OneTimeTearDown]
    public void Teardown()
    {
        TheContainer.Dispose();
    }

    private IContainer TheContainer { get; set; }

    [Test]
    public async Task Test1010_0100_Should_Return_Time_Payload()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var client = scope.Resolve<ServiceClient>();

        var response = await client.Request<Envelope<Dictionary<string,DateTime>>>(FakerService.PingFqn);

        ClassicAssert.IsNotNull(response);
        

    }


    [Test]
    public async Task Test1010_0300_Should_Return_Time_Payload()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var client = scope.Resolve<ServiceClient>();

        ServiceEndpoint ep = new()
        {
            ServiceName = FakerService.ServiceName,
            EndpointName = FakerService.PingFqn,
            BaseAddress = new Uri("http://127.0.0.1:8181/webhooks/"),
            Method = HttpMethod.Post,
            Path = new Uri("Rouchefoucauld", UriKind.Relative),
            Authentication = AuthenticationType.None
        };


        var response = await client.Request<Envelope<Dictionary<string, DateTime>>>(ep);


        ClassicAssert.IsNotNull(response);


    }


    [Test]
    public async Task Test1010_0400_Should_Return_Time_Payload()
    {

        await using var scope = TheContainer.BeginLifetimeScope();


        var client = scope.Resolve<FakerService.IClient>();


        var response = await client.Ping();


        ClassicAssert.IsNotNull(response);
        ClassicAssert.IsNotNull(response.Value);
        ClassicAssert.IsTrue(response.Value.Count == 6);

        Dictionary<string, DateTime> dict = response;

        ClassicAssert.IsNotNull(dict);
        ClassicAssert.IsTrue(dict.Count == 6);


    }







    [Test]
    public async Task Test1010_0100_Should_Return_TestTopic_Payload()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var client = scope.Resolve<ServiceClient>();

        var body = new TestRequest {Name = "Gabby", BirthDate = new DateTime(2009, 4, 13, 13, 30, 0)};

        var response = await client.Request<Envelope<Dictionary<string, DateTime>>>(FakerService.TestTopicFqn, body);

        ClassicAssert.IsNotNull(response);


    }


    [Test]
    public async Task Test1010_0200_Should_Return_TestTopic_Payload()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var client = scope.Resolve<FakerService.IClient>();

        var body = new TestRequest { Name = "Gabby", BirthDate = new DateTime(2009, 4, 13, 13, 30, 0) };

        var response = await client.Test(body);

        ClassicAssert.IsNotNull(response);
        ClassicAssert.IsNotEmpty(response.Details);
        ClassicAssert.IsNotEmpty(response.Details.First().Explanation);

    }


    [Test]
    public async Task Test1010_0200_Should_Return_Email_Payload()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var client = scope.Resolve<ServiceClient>();

        var ep = new ServiceEndpoint
        {
            ServiceName = "Make",
            EndpointName = "PeriodClose",
            BaseAddress = new Uri("https://hook.us1.make.com/xzhon4jn7bgmh23aqvua2jdagnh2e62y/"),
            Path = new Uri("", UriKind.Relative),
            Timeout = TimeSpan.FromSeconds(20),
        };

        var response = await client.RequestAsStream(ep);


        ClassicAssert.IsNotNull(response);


    }


}


public class ServiceTestModule : Module
{

    public string MakeBaseAddress { get; set; } = "https://hook.us1.make.com/";

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.AddServiceAddress("Webhooks", "http://127.0.0.1:8181/webhooks/");
        builder.AddServiceAddress("Make", MakeBaseAddress );

        builder.AddServiceClient(_build);

        builder.RegisterFakerClient();

    }

    private IEnumerable<ServiceEndpoint> _build(IEnumerable<ServiceAddress> addresses)
    {

        var dict = addresses.ToDictionary(e => e.ServiceName, e => e);
        var list = new List<ServiceEndpoint>();


        if( dict.TryGetValue(FakerService.ServiceName, out var faker) )
        {
            list.AddRange( FakerService.GetEndpoints(faker) );
        }

        if (dict.TryGetValue(MakeClient.ServiceName, out var make))
        {
            list.AddRange(MakeClient.GetEndpoints(make));
        }

        return list;

    }


}


public static class FakerService
{


    public static readonly string ServiceName = "Webhooks";

    private const string PingEp = "Rouchefoucauld";
    public static readonly string PingFqn = $"{ServiceName}:{PingEp}";

    private const string TestTopicEp = "TestTopic";
    public static readonly string TestTopicFqn = $"{ServiceName}:{TestTopicEp}";

    public static List<ServiceEndpoint> GetEndpoints( ServiceAddress sda )
    {

        var list = new List<ServiceEndpoint>
        {
            new ()
            {
                ServiceName = ServiceName,
                EndpointName = PingEp,
                BaseAddress = new Uri(sda.Address),
                Method = HttpMethod.Post,
                Path = new Uri("Rouchefoucauld", UriKind.Relative),
                Authentication = AuthenticationType.None
            },
            new ()
            {
                ServiceName = ServiceName,
                EndpointName = TestTopicEp,
                BaseAddress = new Uri(sda.Address),
                Method = HttpMethod.Post,
                Path = new Uri("test-topic", UriKind.Relative),
                Authentication = AuthenticationType.None
            }
        };

        return list;

    }


    public interface IClient
    {

        Task<PingResponse> Ping();

        Task<TestResponse> Test(TestRequest request);

    }

    public static IClient Create(ServiceClient client)
    {

        var definition = new
        {
            Ping = Return<Task<PingResponse>>.Arguments(async () =>
            {
                var res = await client.Request<PingResponse>(PingFqn);
                return res;
            }),
            Test = Return<Task<TestResponse>>.Arguments<TestRequest>(async req =>
            {
                var res = await client.Request<TestResponse>(TestTopicFqn, req);
                return res;
            })
        };

        var impl = definition.ActLike<IClient>();

        return impl;

    }


    public static ContainerBuilder RegisterFakerClient(this ContainerBuilder builder)
    {


        builder.Register(c =>
            {

                var client = c.Resolve<ServiceClient>();

                var impl = Create(client);

                return impl;

            })
            .As<IClient>()
            .InstancePerLifetimeScope();

        return builder;

    }



}


public class PingResponse : Envelope<Dictionary<string,DateTime>>
{

}


public class TestRequest
{

    [DefaultValue(0)]
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; } = DateTime.MinValue;

}

public class TestResponse : Envelope
{

}





public static class MakeClient
{

    public static readonly string ServiceName = "Make";

    private const string SendMailEp = "SendMail";
    public static readonly string SendEmailFqn = $"{ServiceName}:{SendMailEp}";


    public static List<ServiceEndpoint> GetEndpoints( ServiceAddress sda )
    {
        var list = new List<ServiceEndpoint>
        {
            new ()
            {
                ServiceName = ServiceName,
                EndpointName = SendMailEp,
                BaseAddress = new Uri(sda.Address),
                Method = HttpMethod.Post,
                Path = new Uri("ieblmlkxpftcqgg79qxu4qejurwwkpd9", UriKind.Relative),
                Authentication = AuthenticationType.None
            }
        };

        return list;

    }

    public static async Task<EmailResponse> SendEmail(this ServiceClient client, EmailRequest payload)
    {

        var response = await client.Request<EmailResponse>(SendEmailFqn, payload);

        return response;

    }

}


public class EmailRequest
{

    public class BodyModel
    {

        public string Subject { get; set; } = string.Empty;
        public string Salutation { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

    }

    public class AttachmentModel
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }


    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();

    public BodyModel Model { get; set; } = new ();

    public string Template { get; set; } = string.Empty;

    public List<AttachmentModel> Attachments { get; set; } = new();


}

public class EmailResponse
{

    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

}



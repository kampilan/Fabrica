using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Api.Support;
using Fabrica.Http;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Persistence.Http.Mediator;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Rules;
using Fabrica.Test.Models.Patch;
using Fabrica.Tests.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using JetBrains.Annotations;
using NUnit.Framework;
using ModelJsonTypeInfoResolver = Fabrica.Models.Serialization.ModelJsonTypeInfoResolver;
using Module = Autofac.Module;

namespace Fabrica.Tests.Http;


[TestFixture]
public class HttpMediatorTests
{


    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();


        var builder = new ContainerBuilder();

        builder.RegisterModule<TheModule>();

        TheContainer = await builder.BuildAndStart();


    }

    [OneTimeTearDown]
    public void Teardown()
    {

        TheContainer.Dispose();
        WatchFactoryLocator.Factory.Stop();

    }

    private IContainer TheContainer { get; set; }


    [Test]
    public async Task Test0600_0100_QueryPeople()
    {

        using( var scope = TheContainer.BeginLifetimeScope() )
        {

            var filter = RqlFilterBuilder<Person>
                .Where(p => p.FirstName).StartsWith("J")
                .And(p => p.LastName).StartsWith("M");

            var request = QueryEntityRequest<Person>.Where(filter);

            var mm = scope.Resolve<IMessageMediator>();

            var response = await mm.Send(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok);
            Assert.IsNotEmpty(response.Value);

        }


    }


    [Test]
    public async Task Test0600_0110_QueryPeopleByCriteria()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var critera = new PersonCriteria
            {
                FirstName = "J",
                LastName  = "M"
            };

            var request = QueryEntityRequest<Person>.Where( critera );

            var mm = scope.Resolve<IMessageMediator>();

            var response = await mm.Send(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok);
            Assert.IsNotEmpty(response.Value);

        }


    }




    [Test]
    public async Task Test0600_0150_QueryPeopleNoResult()
    {

        using( var scope = TheContainer.BeginLifetimeScope() )
        {

            var filter = RqlFilterBuilder<Person>
                .Where(p => p.FirstName).StartsWith("XXX");

            var request = QueryEntityRequest<Person>.Where(filter);


            var mm = scope.Resolve<IMessageMediator>();

            var response = await mm.Send(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok);
            Assert.IsEmpty(response.Value);

        }


    }



    [Test]
    public async Task Test0600_0200_RetrievePerson()
    {

        using( var scope = TheContainer.BeginLifetimeScope() )
        {


            var filter = RqlFilterBuilder<Person>
                .Where(p => p.LastName).StartsWith("J");

            var request = QueryEntityRequest<Person>.Where(filter);

            var mm = scope.Resolve<IMessageMediator>();

            var response = await mm.Send(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok);
            Assert.IsNotEmpty(response.Value);

            var person = response.Value.First();

            var req2 = RetrieveEntityRequest<Person>.ForUid(person.Uid);
            
            var res2 = await mm.Send(req2);


            Assert.IsNotNull(res2);
            Assert.IsTrue(res2.Ok);
            Assert.IsNotNull(res2.Value);

            Assert.AreEqual(person.Uid, res2.Value.Uid);
            Assert.AreEqual(person.LastName, res2.Value.LastName);


        }


    }


    [Test]
    public async Task Test0600_0250_RetrievePersonNoResult()
    {

        using( var scope = TheContainer.BeginLifetimeScope() )
        {


            var req2 = RetrieveEntityRequest<Person>.ForUid("fgdfgdfgdfgdfgdfgdfg");

            var mm = scope.Resolve<IMessageMediator>();

            var res2 = await mm.Send(req2);


            Assert.IsNotNull(res2);
            Assert.IsFalse(res2.Ok);
            Assert.IsNull(res2.Value);


        }


    }


    [Test]
    public async Task Test0600_0300_UpdatePerson()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {


            var filter = RqlFilterBuilder<Person>
                .Where(p => p.FirstName).StartsWith("J");

            var request = QueryEntityRequest<Person>.Where(filter);

            var mm = scope.Resolve<IMessageMediator>();

            var response = await mm.Send(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok);
            Assert.IsNotEmpty(response.Value);

            var person = response.Value.First();

            person.FirstName = "Mark";


            var req2 = PatchEntityRequest<Person>.FromModel(person);

            var res2 = await mm.Send(req2);


            Assert.IsNotNull(res2);
            Assert.IsTrue(res2.Ok);
            Assert.IsNotNull(res2.Value);

            Assert.AreEqual(person.Uid, res2.Value.Uid);
            Assert.AreEqual(person.LastName, res2.Value.LastName);


            var req3 = RetrieveEntityRequest<Person>.ForUid(res2.Value.Uid);

            var res3 = await mm.Send(req3);


            Assert.IsNotNull(res3);
            Assert.IsTrue(res3.Ok);
            Assert.IsNotNull(res3.Value);

            Assert.AreEqual(person.Uid, res3.Value.Uid);
            Assert.AreEqual(person.LastName, res3.Value.LastName);



        }


    }


//    [Test]
    public async Task Test0700_0100_RpcCall()
    {
        
        await using var scope = TheContainer.BeginLifetimeScope();

        var req = new RepositoryUrlRequest
        {
            FileExtension = "wav",
            GenerateGet = true,
            GeneratePut = true
        };

        var request = new HttpRpcRequest<RepositoryUrlResponse>(req)
        {
            HttpClientName = "Telephony"
        };

        var mediator = scope.Resolve<IMessageMediator>();

        var response = await mediator.Send(request);

        Assert.NotNull(response);
        Assert.IsTrue(response.Ok);

        Assert.NotNull(response.Value);
        Assert.IsNotEmpty(response.Value.GetUrl);
        Assert.IsNotEmpty(response.Value.PutUrl);


    }

}


public class TheModule : Module
{

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.UseRules();

        builder.RegisterAutoMapper(Assembly.GetExecutingAssembly());

        builder.UseModelMeta()
            .AddModelMetaSource(Assembly.GetExecutingAssembly());

        builder.ConfigureJsonSerializerOptions(o =>
        {
            o.WriteIndented = true;
            o.TypeInfoResolver = new ModelJsonTypeInfoResolver();
        });

        builder.UseMediator()
            .AddHttpRpcHandler()
            .AddHttpClientMediatorHandlers();

        builder.AddHttpClient("Api", "https://fabrica.ngrok.io/fake/api/");
        builder.AddHttpClient("Telephony", "https://fabrica.ngrok.io/telephony/api/");



    }

}


public class PersonCriteria: BaseCriteria
{

    [Criterion(Operation = RqlOperator.StartsWith)]
    [CanBeNull] 
    public string FirstName { get; set; }

    [Criterion(Operation = RqlOperator.StartsWith)]
    [CanBeNull]
    public string LastName { get; set; }

}

[HttpRpcRequest("repository")]
public class RepositoryUrlRequest
{

    public string Key { get; set; } = "";
    public string FileExtension { get; set; } = "";
    public string ContentType { get; set; } = "";

    public int TimeToLive { get; set; }

    public bool GenerateGet { get; set; }
    public bool GeneratePut { get; set; }

}

public class RepositoryUrlResponse
{


    public string Key { get; set; } = "";
    public string ContentType { get; set; } = "";

    public DateTime Expiration { get; set; }


    public string GetUrl { get; set; } = "";
    public string PutUrl { get; set; } = "";


}




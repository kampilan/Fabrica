using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Http;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Http.Mediator;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Rules;
using Fabrica.Test.Models.Patch;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using JetBrains.Annotations;
using NUnit.Framework;
using Module = Autofac.Module;

namespace Fabrica.Tests.Http;


[TestFixture]
public class HttpMediatorTests
{


    [OneTimeSetUp]
    public void Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();


        var builder = new ContainerBuilder();

        builder.RegisterModule<TheModule>();

        TheContainer = builder.Build();


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

            var critera = new PersonCritera
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
                .Where(p => p.FirstName).StartsWith("J");

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

            person.LastName = "Mark";


            var req2 = new PatchEntityRequest<Person>();
            req2.Uid = person.Uid;
            req2.FromModel( person );

            var res2 = await mm.Send(req2);


            Assert.IsNotNull(res2);
            Assert.IsTrue(res2.Ok);
            Assert.IsNotNull(res2.Value);

            Assert.AreEqual(person.Uid, res2.Value.Uid);
            Assert.AreEqual(person.LastName, res2.Value.LastName);


        }


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

        builder.UseMediator()
            .AddHttpClientMediatorHandlers();

        builder.AddHttpClient("Api", "https://kampilan.ngrok.io/fake/api/");


    }

}


public class PersonCritera: BaseCriteria
{

    [Criterion(Operation = RqlOperator.StartsWith)]
    [CanBeNull] 
    public string FirstName { get; set; }

    [Criterion(Operation = RqlOperator.StartsWith)]
    [CanBeNull]
    public string LastName { get; set; }

}




[Model]
public class Company : BaseMutableModel<Company>, IRootModel, IExplorableModel
{

    public override long Id { get; protected set; }
    public override string Uid { get; set; } = "";

    public string Name { get; set; } = "";

    public string Address1 { get; set; } = "";
    public string Address2 { get; set; } = "";

    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";

    public string MainPhone { get; set; } = "";
    public string Fax { get; set; } = "";

    public string Website { get; set; } = "";

    public int EmployeeCount { get; set; } = 0;


}
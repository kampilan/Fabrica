using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Http;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Persistence.Http.Mediator;
using Fabrica.Persistence.Http.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
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

            var request = new QueryEntityRequest<Person>();
            request.Where(p => p.FirstName).StartsWith("J");

            var mm = scope.Resolve<IMessageMediator>();

            var response = await mm.Send(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok);
            Assert.IsNotEmpty(response.Value);

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


[Model]
public class Person: BaseMutableModel<Person>, IRootModel, IExplorableModel
{

    public enum GenderKind { Female, Male }


    public override long Id { get; protected set; }
    public override string Uid { get; set; } = "";

    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";

    public GenderKind Gender { get; set; } = GenderKind.Female;


    public DateTime BirthDate { get; set; } = DateTime.Now.AddYears(-25).Date;

    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";

    public decimal Salary { get; set; } = 0;

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
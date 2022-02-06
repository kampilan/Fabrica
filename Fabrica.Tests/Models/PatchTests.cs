using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Bogus;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Rules;
using Fabrica.Test.Models;
using Fabrica.Test.Models.Patch;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;
using IContainer = Autofac.IContainer;
using Module = Autofac.Module;
using Person = Fabrica.Test.Models.Patch.Person;

namespace Fabrica.Tests.Models;


[TestFixture]
public class PatchTests
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

    private Company _buildCompany( int employees, bool asNew=false  )
    {

        var compRules = new Faker<Company>();

        compRules
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.City, f => f.Address.City());

        var company = compRules.Generate();

        var personRules = new Faker<Person>();

        personRules
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName());

        var emps = personRules.Generate(employees);

        company.Employees = emps;

        if( ! asNew )
            company.Post();

        return company;

    }



    [Test]
    public void Test_0501_0100_PatchCreateToRequest()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var company = _buildCompany(2, true);

            var set = PatchSet.Create();
            set.Add(company);

            var json = set.ToJson();

            var set2 = PatchSet.FromJsonMany(json);


            var resolver = scope.Resolve<IPatchResolver>();

            var requests = resolver.Resolve(set2).ToList();

            Assert.IsNotNull(requests);
            Assert.IsNotEmpty(requests);
            Assert.AreEqual(3, requests.Count);

        }


    }




    [Test]
    public void Test_0501_0200_PatchUpdateToRequest()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var company = _buildCompany(2);


            company.Employees.First().FirstName = "Jim";
            company.City = "Vestal";

            var deleted = company.Employees.TakeLast(1).First();

            company.Employees.Remove(deleted);


            var set = PatchSet.Create();
            set.Add(company);

            var json = set.ToJson();

            var set2 = PatchSet.FromJsonMany(json);


            var resolver = scope.Resolve<IPatchResolver>();

            var requests = resolver.Resolve(set2).ToList();

            Assert.IsNotNull(requests);
            Assert.IsNotEmpty(requests);
            Assert.AreEqual(3, requests.Count);

        }


    }


    [Test]
    public async Task Test_0501_0300_Patch()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var model = new Person();

            model.EnterSuspendTracking();
            model.FirstName = "James";
            model.LastName = "Moring";
            model.ExitSuspendTracking();

            model.FirstName = "Jim";


            var comp     = scope.Resolve<IPatchResolver>();
            var requests = comp.Resolve(model);

            var mediator = scope.Resolve<IMessageMediator>();
            var batch = await mediator.Send(requests);

            Assert.IsNotNull( batch );


        }


    }



}



public class TheModule : Module
{

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.UseRules();

        builder.RegisterAutoMapper(typeof(IAssemblyFinder).Assembly);

        builder.UseModelMeta()
            .AddModelMetaSource(typeof(IAssemblyFinder).Assembly);

        builder.UseMediator(typeof(IAssemblyFinder).Assembly);


        builder.Register(c =>
            {
                var corr = c.Resolve<ICorrelation>();
                var comp = new MediatorRequestFactory( corr );
                return comp;
            })
            .As<IMediatorRequestFactory>()
            .SingleInstance();

        builder.Register(c =>
            {

                var corr = c.Resolve<ICorrelation>();
                var meta = c.Resolve<IModelMetaService>();
                var mediator = c.Resolve<IMessageMediator>();
                var factory = c.Resolve<IMediatorRequestFactory>();

                var comp = new PatchResolver(corr, meta, mediator, factory);
                return comp;



            })
            .AsSelf()
            .As<IPatchResolver>()
            .InstancePerLifetimeScope();

    }


}


public class LocalMediatorRequestFactory: MediatorRequestFactory
{

    public LocalMediatorRequestFactory(ICorrelation correlation) : base(correlation)
    {
    }

    public ICreateEntityRequest GetCustomCreateEntityRequest( Type entity )
    {

        switch (entity)
        {
            case not null when entity == typeof(Person):
                return new CreateEntityRequest<Person>();
            case not null when entity == typeof(Company):
                return new CreateEntityRequest<Company>();
            default:
                return null;
        }
        
        

    }




}
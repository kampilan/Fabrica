using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Rules;
using Fabrica.Test.Models;
using Fabrica.Test.Models.Handlers;
using Fabrica.Test.Models.Patch;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using MediatR;
using NUnit.Framework;
using IContainer = Autofac.IContainer;
using Module = Autofac.Module;

namespace Fabrica.Tests.Models;


[TestFixture]
public class PatchTests
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
    public void Test_0501_0100_PatchCreateToRequest()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var model = new Person();
            model.Added();

            model.FirstName = "James";
            model.LastName = "Moring";

            var set = PatchSet.Create();
            set.Add(model);

            var meta    = scope.Resolve<IModelMetaService>();
            var factory = scope.Resolve<IMediatorRequestFactory>();

            var patch = set.GetPatches().First();

            var type = meta.GetMetaFromAlias(patch.Model);
            var request = factory.GetCreateRequest(type.Target, patch.Uid, patch.Properties);

            Assert.IsNotNull(request);
            Assert.IsInstanceOf<ICreateEntityRequest>(request);
            Assert.IsInstanceOf<CreateEntityRequest<Person>>(request);
            Assert.IsNotEmpty(request.Delta);
            Assert.AreEqual(2, request.Delta.Count);

        }


    }


    [Test]
    public void Test_0501_0100_PatchUpdateToRequest()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var model = new Person();

            model.EnterSuspendTracking();
            model.FirstName = "James";
            model.LastName = "Moring";
            model.ExitSuspendTracking();

            model.FirstName = "Jim";


            var set = PatchSet.Create();
            set.Add(model);

            var meta = scope.Resolve<IModelMetaService>();
            var factory = scope.Resolve<IMediatorRequestFactory>();

            var patch = set.GetPatches().First();

            var type = meta.GetMetaFromAlias(patch.Model);
            var request = factory.GetUpdateRequest(type.Target, patch.Uid, patch.Properties);

            Assert.IsNotNull(request);
            Assert.IsInstanceOf<IUpdateEntityRequest>(request);
            Assert.IsInstanceOf<UpdateEntityRequest<Person>>(request);
            Assert.IsNotEmpty(request.Delta);
            Assert.AreEqual( 1, request.Delta.Count);

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

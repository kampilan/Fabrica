using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using Fabrica.One.Orchestrator.Aws.Configuration;
using Fabrica.One.Plan;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.One.Orchestrator.Aws.Tests;

#nullable disable

[TestFixture]
public class OrchestratorTests
{

    [SetUp]
    public async Task Setup()
    {


        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Debug, Color.Azure);

        maker.Build();



        var module = new OneOrchestratorModule();

        module.Profile = "fortium";
        module.RegionName = "us-east-1";
        module.RunningOnEC2 = false;

        module.OneRoot = @"e:\fabrica-one";
        
        module.AppConfigPlanSourceApplication = "PartnerConnect";
        module.AppConfigPlanSourceConfiguration = "Mission-Development";
        module.AppConfigPlanSourceEnvironment = "Development";

        module.RepositoryBucketName = "connect-appliance-repository";


        var builder = new ContainerBuilder();
        builder.RegisterModule(module);

        TheContainer = await builder.BuildAndStart();

    }


    [TearDown]
    public void Teardown()
    {
        TheContainer?.Dispose();
        TheContainer = null;
    }

    private IContainer TheContainer { get; set; }


    [Test]
    public async Task Test0900_Source_Have_Updated_Non_Empty_Plan()
    {

        using( var scope = TheContainer.BeginLifetimeScope() )
        {

            var source = scope.Resolve<IPlanSource>();

            var updated = await source.HasUpdatedPlan();
            Assert.IsTrue(updated);

            var factory = scope.Resolve<IPlanFactory>();

            var plan = await factory.Create(source, true);

            Assert.IsNotNull(plan);
            Assert.IsNotEmpty(plan.Deployments);


        }    


    }


    [Test]
    public async Task Test0910_Should_Create_Repository_Version()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var source = scope.Resolve<IPlanSource>();

            var updated = await source.HasUpdatedPlan();
            Assert.IsTrue(updated);

            var factory = scope.Resolve<IPlanFactory>();

            var plan = await factory.Create(source, true);

            await factory.CreateRepositoryVersion(plan);

            Assert.IsNotEmpty(plan.RepositoryVersion);

            Assert.IsNotNull(plan);
            Assert.IsNotEmpty(plan.Deployments);

            var loader    = scope.Resolve<IApplianceLoader>();
            var installer = scope.Resolve<IApplianceInstaller>();

            await loader.Clean(plan);
            await installer.Clean(plan);

            var unit = plan.Deployments.First();

            await loader.Load(plan, unit);
            await installer.Install(plan, unit);

            var writer = scope.Resolve<IPlanWriter>();

            await factory.Save(plan, writer);



        }


    }


    [Test]
    public async Task Test0920_Orchestrator_Should_Create_Repository_Version()
    {

        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var orch = scope.Resolve<MissionOrchestrator>();

            await orch.CheckForUpdatedPlan();

            Assert.Pass();

        }


    }






}
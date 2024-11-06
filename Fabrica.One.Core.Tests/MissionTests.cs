using System;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Configuration;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.One.Core.Tests;

[TestFixture]

public class MissionTests: BaseOneTest
{


    //[Test]
    public async Task Test0600_Load_Module()
    {

        var module = new OneMissionModule
        {
            RepositoryRoot = @"z:\repository",
            InstallationRoot = @"c:\appliances\installations"
        };

        var builder = new ContainerBuilder();
        builder.RegisterModule(module);

        var container = builder.Build();

        ClassicAssert.IsNotNull(container);

        await container.DisposeAsync();

    }


    [Test]
    public async Task Test0610_Mission_Should_Start()
    {

        var source = await OneAppliancePlanSourceWithGoodChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);


        var loader    = new FileSysApplianceLoader();
        var installer = new ZipInstaller();
        var appFact   = new ApplianceFactory();

        var mission = new Mission(plan, loader, installer, appFact);


        mission.Reset();

        var result1 = mission.Clean();
        var result2 = mission.Deploy();
        var result3 = mission.Start();

        ClassicAssert.IsNotNull(result1);
        ClassicAssert.IsTrue(result1.Successful);

        ClassicAssert.IsNotNull(result2);
        ClassicAssert.IsTrue(result2.Successful);

        ClassicAssert.IsNotNull(result3);
        ClassicAssert.IsTrue(result3.Successful);

        await Task.Delay(TimeSpan.FromSeconds(5));


        var json = mission.GetStatusAsJson();
        ClassicAssert.IsNotEmpty(json);


        var result4 = mission.Stop();

        ClassicAssert.IsNotNull(result4);
        ClassicAssert.IsTrue(result4.Successful);


    }


    [Test]
    public async Task Test0620_Mission_Should_Start_Empty_Plan()
    {

        var source = await EmptyPlanSource();
        var factory = GetFactory();

        var plan = await factory.Create(source,true);


        var loader = new FileSysApplianceLoader();
        var installer = new ZipInstaller();
        var appFact = new ApplianceFactory();

        var mission = new Mission(plan, loader, installer, appFact);

        var result1 = mission.Clean();
        var result2 = mission.Deploy();
        var result3 = mission.Start();

        ClassicAssert.IsNotNull(result1);
        ClassicAssert.IsTrue(result1.Successful);

        ClassicAssert.IsNotNull(result2);
        ClassicAssert.IsTrue(result2.Successful);

        ClassicAssert.IsNotNull(result3);
        ClassicAssert.IsTrue(result3.Successful);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var json = mission.GetStatusAsJson();
        ClassicAssert.IsNotEmpty(json);


        var result4 = mission.Stop();

        ClassicAssert.IsNotNull(result4);
        ClassicAssert.IsTrue(result4.Successful);


    }






}
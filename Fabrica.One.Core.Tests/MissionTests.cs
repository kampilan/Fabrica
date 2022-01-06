using System;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Configuration;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using NUnit.Framework;

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

        Assert.IsNotNull(container);

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

        Assert.IsNotNull(result1);
        Assert.IsTrue(result1.Successful);

        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.Successful);

        Assert.IsNotNull(result3);
        Assert.IsTrue(result3.Successful);

        await Task.Delay(TimeSpan.FromSeconds(5));


        var json = mission.GetStatusAsJson();
        Assert.IsNotEmpty(json);


        var result4 = mission.Stop();

        Assert.IsNotNull(result4);
        Assert.IsTrue(result4.Successful);


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

        Assert.IsNotNull(result1);
        Assert.IsTrue(result1.Successful);

        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.Successful);

        Assert.IsNotNull(result3);
        Assert.IsTrue(result3.Successful);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var json = mission.GetStatusAsJson();
        Assert.IsNotEmpty(json);


        var result4 = mission.Stop();

        Assert.IsNotNull(result4);
        Assert.IsTrue(result4.Successful);


    }






}
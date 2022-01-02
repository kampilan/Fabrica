﻿using System;
using System.Threading.Tasks;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using NUnit.Framework;

namespace Fabrica.One.Core.Tests;


[TestFixture]
public class ApplianceTests: BaseOneTest
{

    [Test]
    public async Task Test0700_Appliance_Should_Start()
    {

        var source = await OneAppliancePlanSourceWithGoodChecksum();
        var strm = await source.GetSource();

        var factory = GetFactory();

        var plan = factory.Create(strm);
        var unit = plan.Deployments[0];

        var loader = new FileSysApplianceLoader();
        await loader.Clean(plan);
        await loader.Load(plan, unit);

        var installer = new ZipInstaller();
        await installer.Clean(plan);
        await installer.Install(plan, unit);


        var app = new Appliance(unit);

        await app.Start();
        var started = app.WaitForStart(TimeSpan.FromSeconds(10));

        var std = app.GetStdOutput();
        var err = app.GetErrorOutput();

        Assert.IsTrue(started);


        await Task.Delay(TimeSpan.FromSeconds(60));

        app.Stop();

    }



}
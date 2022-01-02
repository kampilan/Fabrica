using System;
using System.IO;
using Fabrica.One.Plan;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Fabrica.One.Core.Tests;


public class PlanSourceTests : BaseOneTest
{


    [Test]
    public async Task Test0500_MemoryPlanSource_Should_Report_Being_Updated()
    {

        var source = new MemoryPlanSource();
        source.CheckInterval = TimeSpan.FromSeconds(1);

        Assert.IsFalse(await source.HasUpdatedPlan());

        await using var strm = new MemoryStream();
        await using var writer = new StreamWriter(strm);
        await writer.WriteAsync("Testing");
        await writer.FlushAsync();

        strm.Seek(0, SeekOrigin.Begin);

        source.CopyFrom(strm);

        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.IsTrue(await source.HasUpdatedPlan());


    }

    [Test]
    public async Task Test0510_MemoryPlanSource_Should_Report_Being_Reloaded()
    {

        var source = new MemoryPlanSource();
        source.CheckInterval = TimeSpan.FromSeconds(1);


        Assert.IsFalse(await source.HasUpdatedPlan());


        await using var strm = new MemoryStream();
        await using var writer = new StreamWriter(strm);
        await writer.WriteAsync("Testing");
        await writer.FlushAsync();

        strm.Seek(0, SeekOrigin.Begin);

        source.CopyFrom(strm);

        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.IsTrue(await source.HasUpdatedPlan());

        await source.GetSource();

        Assert.IsFalse(await source.HasUpdatedPlan());

        await source.Reload();

        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.IsTrue(await source.HasUpdatedPlan());


    }





    [Test]
    public async Task Test0520_FilePlanSource_Should_Report_Being_Updated()
    {

        var source = new FilePlanSource
        {
            FileDir = @"c:\temp",
            FileName = "test-mission-plan.json",
            CheckInterval = TimeSpan.FromSeconds(1)
        };

        await source.Start();

        Assert.IsTrue(await source.HasUpdatedPlan());

        await source.GetSource();

        Assert.IsFalse(await source.HasUpdatedPlan());

        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.IsFalse(await source.HasUpdatedPlan());

        await using var fi = new FileStream(@"c:\temp\test-mission-plan.json", FileMode.Open, FileAccess.Read);
        var ms = new MemoryStream();
        await fi.CopyToAsync(ms);
        await fi.DisposeAsync();

        ms.Seek(0, SeekOrigin.Begin);

        await using var fo = new FileStream(@"c:\temp\test-mission-plan.json", FileMode.Create, FileAccess.Write);
        await ms.CopyToAsync(fo);
        await fo.DisposeAsync();

        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.IsTrue(await source.HasUpdatedPlan());

    }


    [Test]
    public async Task Test0530_FilePlanSource_Should_Report_Being_Reloaded()
    {

        var source = new FilePlanSource
        {
            FileDir = @"c:\\temp",
            FileName = "test-mission-plan.json",
            CheckInterval = TimeSpan.FromSeconds(1)
        };

        await source.Start();

        Assert.IsTrue(await source.HasUpdatedPlan());

        await source.GetSource();

        Assert.IsFalse(await source.HasUpdatedPlan());

        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.IsFalse(await source.HasUpdatedPlan());

        source.Touch();

        await Task.Delay(TimeSpan.FromSeconds(1));

        Assert.IsTrue(await source.HasUpdatedPlan());

    }

}







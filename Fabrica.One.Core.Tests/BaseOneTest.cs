
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Fabrica.One.Plan;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.One.Core.Tests;

public class BaseOneTest
{

    [OneTimeSetUp]
    public void OneTimeSetup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Debug, Color.Azure);

        maker.Build();

    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {

        WatchFactoryLocator.Factory.Stop();

    }

    protected Task<IPlanSource> EmptyPlanSource()
    {
        var source = new MemoryPlanSource();
        return Task.FromResult((IPlanSource)source);
    }


    protected async Task<IPlanSource> EmptyJsonPlanSource()
    {

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync("{}");
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }


    protected async Task<IPlanSource> BadJsonPlanSource()
    {

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync("{ Test= }");
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }





    protected async Task<IPlanSource> NoAppliancePlanSource()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = Guid.NewGuid().ToString(),
            Name = "Test",
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };

        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }

    protected async Task<IPlanSource> OneAppliancePlanSourceWithNoChecksum()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = "1234567890",
            Name = "Test",
            DeployAppliances = true,
            StartAppliances = true,
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };


        var deployment = new DeploymentUnit
        {
            Name = "fabrica-monitor",
            Alias = "fabrica-monitor-1",
            Build = "05324",
            Checksum = "",
            Assembly = "Appliance",
            Deploy = sp.DeployAppliances
        };

        var config = new JsonObject
        {
            ["Test"] = "Cool"
        };

        deployment.Configuration = config;


        sp.Deployments.Add(deployment);


        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }


    protected async Task<IPlanSource> OneAppliancePlanSourceWithNoChecksumNoDeploy()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = "1234567890",
            Name = "Test",
            DeployAppliances = false,
            StartAppliances = true,
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };


        var deployment = new DeploymentUnit
        {
            Name = "fabrica-monitor",
            Alias = "fabrica-monitor-1",
            Build = "05324",
            Checksum = "",
            Assembly = "Appliance",
            Deploy = sp.DeployAppliances
        };

        var config = new JsonObject
        {
            ["Test"] = "Cool"
        };

        deployment.Configuration = config;


        sp.Deployments.Add(deployment);


        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }



    protected async Task<IPlanSource> OneAppliancePlanSourceWithGoodChecksum()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = "1234567890",
            Name = "Test",
            DeployAppliances = true,
            StartAppliances = true,
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };


        var deployment = new DeploymentUnit
        {
            Name = "fabrica-monitor",
            Alias = "fabrica-monitor-1",
            Build = "05326",
            Checksum = "8dc8ebeac47327b168b93c536bbf01ff222c14e8bf79b444203bb60cd0731351",
            Assembly = "Appliance",
            Deploy = sp.DeployAppliances,
            ShowWindow = false
        };

        var config = new JsonObject
        {
            ["ListeningPort"] = "8888",
            ["HealthcheckRoute"] = "/gabby"
        };

        deployment.Configuration = config;


        sp.Deployments.Add(deployment);


        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }





    protected async Task<IPlanSource> OneApplianceBadChecksumPlanSource()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = "1234567890",
            DeployAppliances = true,
            StartAppliances = true,
            Name = "Test",
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };


        var deployment = new DeploymentUnit
        {
            Name = "fabrica-monitor",
            Alias = "fabrica-monitor-1",
            Build = "05324",
            Checksum = "XgyPqsiJXexXXaNIKx+eDgP5t122w=",
            Assembly = "Appliance",
            Deploy = sp.DeployAppliances
        };

        var config = new JsonObject
        {
            ["Test"] = "Cool"
        };

        deployment.Configuration = config;


        sp.Deployments.Add(deployment);


        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }






    protected async Task<IPlanSource> OneNoExistAppliancePlanSource()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = "1234567890",
            Name = "Test",
            DeployAppliances = true,
            StartAppliances = true,
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };


        var deployment = new DeploymentUnit
        {
            Name = "fabrica-shtarker",
            Alias = "fabrica-shtarker-1",
            Build = "05324",
            Checksum = "",
            Assembly = "Appliance",
            Deploy = sp.DeployAppliances
        };

        var config = new JsonObject
        {
            ["Test"] = "Cool"
        };

        deployment.Configuration = config;


        sp.Deployments.Add(deployment);


        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }


    protected async Task<IPlanSource> OneBogusAppliancePlanSource()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = "1234567890",
            Name = "Test",
            DeployAppliances = true,
            StartAppliances = true,
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };


        var deployment = new DeploymentUnit
        {
            Name = "fabrica-monitor",
            Alias = "fabrica-monitor-1",
            Build = "bogus",
            Checksum = "",
            Assembly = "Appliance",
            Deploy = sp.DeployAppliances
        };

        var config = new JsonObject
        {
            ["Test"] = "Cool"
        };

        deployment.Configuration = config;


        sp.Deployments.Add(deployment);


        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }




    protected async Task<IPlanSource> MultiAppliancePlanSource()
    {

        var sp = new PlanImpl
        {
            RepositoryVersion = Guid.NewGuid().ToString(),
            Name = "Test",
            DeployAppliances = true,
            StartAppliances = true,
            WaitForDeploySeconds = 10,
            WaitForStartSeconds = 10,
            WaitForStopSeconds = 10

        };

        var deployment = new DeploymentUnit
        {
            Name = "Shtaker",
            Alias = "Shtarker",
            Build = "latest",
            Checksum = "1234567890",
            Assembly = "Appliance",
            Deploy = sp.DeployAppliances
        };

        var config = new JsonObject
        {
            ["Test"] = "Cool"
        };

        deployment.Configuration = config;


        sp.Deployments.Add(deployment);

        var deployment2 = new DeploymentUnit
        {
            Name = "ShtakerZZ",
            Alias = "ShtarkerZZZ",
            Build = "local",
            Checksum = "1234567893",
            Assembly = "Tester",
            Deploy = sp.DeployAppliances
        };

        var config2 = new JsonObject
        {
            ["Test"] = "lame"
        };

        deployment2.Configuration = config2;

        sp.Deployments.Add(deployment2);


        var json = JsonSerializer.Serialize(sp);

        await using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);

        var source = new MemoryPlanSource();
        source.CopyFrom(stream);

        return source;

    }


    protected IPlanFactory GetFactory()
    {
        var factory = new JsonPlanFactory(@"e:\fabrica-one\repository", @"e:\fabrica-one\installations");
        return factory;
    }



}
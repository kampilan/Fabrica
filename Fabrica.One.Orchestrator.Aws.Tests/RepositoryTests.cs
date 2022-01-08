using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fabrica.One.Orchestrator.Aws.Repository;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.One.Orchestrator.Aws.Tests;


[TestFixture]
public class RepositoryTests
{

    [OneTimeSetUp]
    public void Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Debug, Color.Azure);

        maker.Build();


        Repository = new S3Repository
        {
            ProfileName = "kampilan",
            BucketName = "pondhawk-appliance-repository",
            RegionName = "us-east-2",
            RunningOnEc2 = false
        };

    }

    private S3Repository Repository { get; set; } = null!;


    [Test]
    public async Task Test1100_Missions_Should_Populate()
    {

        var missions = (await Repository.GetMissions()).ToList();
        
        Assert.IsNotEmpty(missions);

    }


    [Test]
    public async Task Test1110_Builds_Should_Populate()
    {

        var builds = (await Repository.GetBuilds()).ToList();

        Assert.IsNotEmpty(builds);

    }



    [Test]
    public async Task Test1100_New_Mission_Should_Save()
    {

        await Repository.GetMissions();
        var all = await Repository.GetBuilds();

        var nm = await Repository.CreateMission("cool");

        Assert.IsNotNull(nm);
        Assert.AreEqual("cool", nm.Name );
        Assert.IsEmpty(nm.Deployments);


        var build = all.First();
        var nd = nm.AddDeployment( build );

        var dict = new Dictionary<string, object>
        {
            ["SomeValue"]    = "Cool",
            ["AnotherValue"] = 23456,
            ["Stuff"] = new Dictionary<string,object>{["Deep"] = true}
        };

        nd.SetEnvironmentConfiguration(dict);


        Assert.IsNotEmpty(nm.Deployments);

        await Repository.Save(nm);

        await Repository.Delete(nm);


    }







}
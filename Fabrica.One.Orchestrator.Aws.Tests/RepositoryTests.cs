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

        Manager = new S3RepositoryManager
        {
            RunningOnEc2 = false
        };

    }

    private S3RepositoryManager Manager { get; set; } = null!;



    [Test]
    public async Task Test1100_Repositories_Should_Populate()
    {

        var repos = (await Manager.GetRepositories()).ToList();

        Assert.IsNotEmpty(repos);

    }


    [Test]
    public async Task Test1110_Missions_Should_Populate()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        Assert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);

        var missions = (await Manager.GetMissions()).ToList();
        
        Assert.IsNotEmpty(missions);

    }


    [Test]
    public async Task Test1120_Builds_Should_Populate()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        Assert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);


        var builds = (await Manager.GetBuilds()).ToList();

        Assert.IsNotEmpty(builds);

    }



    [Test]
    public async Task Test1130_New_Mission_Should_Save()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        Assert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);



        await Manager.GetMissions();
        var all = await Manager.GetBuilds();

        var nm = await Manager.CreateMission("cool");

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

        await Manager.Save(nm);

        await Manager.Delete(nm);


    }




}
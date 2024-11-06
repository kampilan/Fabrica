using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fabrica.One.Support.Aws.Repository;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.One.Support.Aws.Tests;


[TestFixture]
public class RepositoryTests
{

    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Debug, Color.Azure);

        maker.Build();

        Manager = new S3RepositoryManager
        {
            RunningOnEc2 = false,
            ProfileFilter = "kampilan"
        };

        await Manager.Start();
    }

    private S3RepositoryManager Manager { get; set; } = null!;



    [Test]
    public async Task Test1100_Repositories_Should_Populate()
    {

        var repos = (await Manager.GetRepositories()).ToList();

        ClassicAssert.IsNotEmpty(repos);

    }


    [Test]
    public async Task Test1110_Missions_Should_Populate()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        ClassicAssert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);

        var missions = (await Manager.GetMissions()).ToList();
        
        ClassicAssert.IsNotEmpty(missions);

    }


    [Test]
    public async Task Test1120_Builds_Should_Populate()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        ClassicAssert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);


        var builds = (await Manager.GetBuilds()).ToList();

        ClassicAssert.IsNotEmpty(builds);

    }



    [Test]
    public async Task Test1130_New_Mission_Should_Save()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        ClassicAssert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);



        await Manager.GetMissions();
        var all = await Manager.GetBuilds();

        var nm = await Manager.CreateMission( "cool", "development" );

        ClassicAssert.IsNotNull(nm);
        ClassicAssert.AreEqual("cool", nm.Name );
        ClassicAssert.IsEmpty(nm.Deployments);


        var build = all.First();
        var nd = nm.AddDeployment( build );

        var dict = new Dictionary<string, object>
        {
            ["SomeValue"]    = "Cool",
            ["AnotherValue"] = 23456,
            ["Stuff"] = new Dictionary<string,object>{["Deep"] = true}
        };

        nd.SetConfiguration(dict);


        ClassicAssert.IsNotEmpty(nm.Deployments);


        var dp = nm.Deployments.First();

        var yaml = dp.GetConfigurationAsJson();



        await Manager.Save(nm);

        await Manager.Delete(nm);


    }




    [Test]
    public async Task Test1140_Mission_Should_Load()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        ClassicAssert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);



        var missions = await Manager.GetMissions();

        var mm = missions.FirstOrDefault(m => m.Fqmn == "Fake-Local");

        ClassicAssert.IsNotNull(mm);
        ClassicAssert.IsNotEmpty(mm.Deployments);

        var dm = mm.Deployments.FirstOrDefault();

        ClassicAssert.IsNotNull(dm);

        var cj = dm.ConfigurationAsJson;
        ClassicAssert.IsNotNull(cj);
        ClassicAssert.IsNotEmpty(cj);

        var dict = new Dictionary<string, object>
        {
            ["Test"] = "Cool",
            ["Name"] = "Gabby",
            ["Age"] = 12
        };

        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions(JsonSerializerDefaults.General) {WriteIndented = true});

        dm.ConfigurationAsJson = json;

        mm.Post();

        var mj = JsonSerializer.Serialize(mm, new JsonSerializerOptions(JsonSerializerDefaults.General) {WriteIndented = true});

        ClassicAssert.IsNotNull(mj);
        ClassicAssert.IsNotEmpty(mj);


    }


    [Test]
    public async Task Test1140_Mission_Build_Usage_Should_Load()
    {

        var repo = (await Manager.GetRepositories(r => r.Description.Contains("pondhawk-"))).FirstOrDefault();

        ClassicAssert.IsNotNull(repo);

        await Manager.SetCurrentRepository(repo);


        var builds = await Manager.GetMissionBuildUsage();

        ClassicAssert.IsNotNull(builds);
        ClassicAssert.IsNotEmpty(builds);


    }






}
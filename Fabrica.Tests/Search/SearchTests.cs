using System;
using Autofac;
using Bogus;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Fabrica.Aws;
using Fabrica.Search;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Lifti;

namespace Fabrica.Tests.Search;


public static class DocumentGenerator
{


    private static HashSet<string> IndustryKeywords { get; } = new()
    {
        "Retail", "Manufacturing", "Farming", "Restaurant", "Automotive", "Apparel", "Technology", "Medical",
        "Aquaculture", "Chemical", "SaaS", "Utilities", "Accounting", "Banking", "Brokerage", "Financial",
        "Construction", "Landscaping", "Architecture", "Petroleum"
    };

    private static HashSet<string> WorkKeywords { get; } = new()
    {
        "Retail", "Manufacturing", "Farming", "Restaurant", "Automotive", "Apparel", "Technology", "Medical"
    };

    private static HashSet<string> TechnicalKeywords { get; } = new()
    {
        "Microsoft", "Oracle", "SAP", "SQL Server", "MySql", "Azure", "Amazon", "Google", "Windows", "Linux", "ASP.NET", "Angular", "TeamCity", "QuickBooks", "Epicor", "Java", ".NET", "Rust", "C++", "Go", "Apache","Nginx", "Tomcat", "Red Hat"
    };


    public static List<UserDocument> BuildDocuments( int count=2000 )
    {

        long id = 0;

        var docRules = new Faker<UserDocument>();

        docRules.RuleFor(d => d.Id, () => ++id);
        docRules.RuleFor(d => d.FirstName, (f) => f.Name.FirstName());
        docRules.RuleFor(d => d.LastName, (f) => f.Name.LastName());
        docRules.RuleFor(d => d.City, (f, _) => f.Address.City());
        docRules.RuleFor(d => d.City, (f, _) => f.Address.StateAbbr());
        docRules.RuleFor(d => d.LanguagesSpoken, (f, _) => { return string.Join(", ", f.Make(2, () => f.PickRandomParam("French", "Spanish", "German", "Dutch", "Italian", "Russian", "Greek"))); });
        docRules.RuleFor(d => d.IndustryExperience, (f, _) => { return string.Join(", ", f.Make(8, () => f.PickRandom(IndustryKeywords.Select(s=>s)))); });
        docRules.RuleFor(d => d.WorkExperience, (f, _) => { return string.Join(", ", f.Make(8, () => f.PickRandom(WorkKeywords.Select(s => s)))); });
        docRules.RuleFor(d => d.TechnicalExperience, (f, _) => { return string.Join(", ", f.Make(8, () => f.PickRandom(TechnicalKeywords.Select(s => s)))); });


        return docRules.Generate(count);

    }



}




public class SearchTests
{




    protected class TheModule : Module, IAwsCredentialConfiguration
    {

        public string AwsProfileName { get; set; } = "kampilan";
        public bool UseLocalAwsCredentials { get; set; } = true;


        protected override void Load(ContainerBuilder builder)
        {


            builder.UseAws(this);


            builder.Register(c =>
                {

                    var client = c.Resolve<IAmazonS3>();

                    var comp = new UserSearchProvider(client)
                    {
                        BucketName = "after-serene-permanent",
                        IndexRoot = "indexes"
                    };

                    return comp;

                })
                .AsSelf()
                .As<ISearchProvider<ResourceAvailabilityTest>>()
                .As<IRequiresStart>()
                .SingleInstance()
                .AutoActivate();

        }

    }


    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.LightBlue);

        maker.Build();


        var builder = new ContainerBuilder();

        var module = new TheModule();

        builder.RegisterModule(module);

        RootScope = await builder.BuildAndStart();

    }

    [OneTimeTearDown]
    public void Teardown()
    {

    }

    private ILifetimeScope RootScope { get; set; }



    [Test]
    public async Task Test08120_0100_Build_Index()
    {


        await using var scope = RootScope.BeginLifetimeScope();

        var provider = scope.Resolve<ISearchProvider<ResourceAvailabilityTest>>();

        var res = await provider.Search("Apparel Amazon French");

        Assert.IsNotNull(res);

        Console.Out.WriteLine($"{res.Count()} items matched");

        var sw = new Stopwatch();
        sw.Start();

        for (var i = 0; i < 100; i++)
        {
            var res2 = await provider.Search("Chemical Azure German");
        }

        sw.Stop();

        await Console.Out.WriteLineAsync($"{sw.ElapsedMilliseconds} msec(s)");


    }





    [Test]
    public async Task Test08120_0200_Build_Index()
    {


        await using var scope = RootScope.BeginLifetimeScope();

        var provider = scope.Resolve<ISearchProvider<ResourceAvailabilityTest>>();

        var res = await provider.Search("Apparel Amazon French");

        Assert.IsNotNull(res);

        Console.Out.WriteLine($"{res.Count()} items matched");

        var sw = new Stopwatch();
        sw.Start();

        for (var i = 0; i < 100; i++)
        {
            var res2 = await provider.Search("Chemical Azure German");
        }

        sw.Stop();

        await Console.Out.WriteLineAsync( $"{sw.ElapsedMilliseconds} msec(s)");


    }



}

public class ResourceAvailabilityTest
{
}

public class UserSearchProvider : AbstractClusterSearchProvider<ResourceAvailabilityTest>
{
    
    
    public UserSearchProvider( IAmazonS3 client ) : base( new Correlation(), client)
    {
    }

    protected override Task<IEnumerable<InputDocument>> GetInputs()
    {

        var docs = DocumentGenerator.BuildDocuments(20000);

        var inputs = docs.Select(doc =>
        {

            var id = new InputDocument
            {
                Key = new InputKey("Resource", doc.Id),
                Keywords =
                {
                    ["City"] = doc.City,
                    ["State"] = doc.State,
                    ["Languages"] = doc.LanguagesSpoken,
                    ["Industry"] = doc.IndustryExperience,
                    ["Work"] = doc.WorkExperience,
                    ["Technical"] = doc.TechnicalExperience
                }
            };

            return id;

        });

        return Task.FromResult( inputs );

    }

    protected override FullTextIndex<InputKey> CreateIndexDefinition()
    {


        var builder = new FullTextIndexBuilder<InputKey>()
            .WithObjectTokenization<InputDocument>(
                options => options
                    .WithKey(u => u.Key)
                    .WithDynamicFields("Keywords", i => i.Keywords)
            );

        return builder.Build();


/*
        var builder = new FullTextIndexBuilder<InputKey>()
            .WithObjectTokenization<UserDocument>(
                options => options
                    .WithKey(u => new InputKey(u.Entity, u.Id))
                    .WithField("City", b => b.City)
                    .WithField("State", b => b.State)
                    .WithField("Languages", b => b.LanguagesSpoken, t => t.SplitOnPunctuation().CaseInsensitive())
                    .WithField("Industry", b => b.IndustryExperience, t => t.SplitOnPunctuation().CaseInsensitive())
                    .WithField("Work", b => b.WorkExperience, t => t.SplitOnPunctuation().CaseInsensitive())
                    .WithField("Technical", b => b.TechnicalExperience, t => t.SplitOnPunctuation().CaseInsensitive())
            );

        return builder.Build();
*/

    }



}




public class UserDocument
{

    public string Entity { get; set; } = "User";
    public long Id { get; set; }

    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    public string City { get; set; } = "";
    public string State { get; set; } = "";

    public string LanguagesSpoken { get; set; } = "";

    public string IndustryExperience { get; set; } = "";

    public string WorkExperience { get; set; } = "";

    public string TechnicalExperience { get; set; } = "";


}

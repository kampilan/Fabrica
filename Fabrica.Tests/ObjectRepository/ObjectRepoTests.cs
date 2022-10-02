using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Repository;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;

namespace Fabrica.Tests.ObjectRepository;

[TestFixture]
public class ObjectRepoTests
{


    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();


        var builder = new ContainerBuilder();

        builder.RegisterModule(new RepoTestModule());

        TheContainer = await builder.BuildAndStart();

    }

    [OneTimeTearDown]
    public void Teardown()
    {

        TheContainer?.Dispose();
        TheContainer = null;

        WatchFactoryLocator.Factory.Stop();

    }


    private IContainer TheContainer { get; set; }


    [Test]
    public async Task Test_1100_0100_PutPermanentTest()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var repo = scope.Resolve<IObjectRepository>();


        var fi = new FileInfo("c:/temp/1705form_CFB.pdf");

        await using var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);

        var key = await repo.Put(b =>
        {
            b.Content   = fs;
            b.Extension = fi.Extension;
            b.Rewind    = false;
        });


        Assert.IsNotNull(key);
        Assert.IsNotEmpty(key);

        var exists = await repo.Get(b =>
        {
            b.Key     = key;
            b.Content = new MemoryStream();
        });

        Assert.IsTrue(exists);

    }

    [Test]
    public async Task Test_1100_0200_PutTransientTest()
    {

        await using var scope = TheContainer.BeginLifetimeScope();

        var repo = scope.Resolve<IObjectRepository>();


        var fi = new FileInfo("c:/temp/1705form_CFB.pdf");

        await using var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);

        void MakePut( PutOptions ops )
        {
            ops.Transient = true;
            ops.Content   = fs;
            ops.Extension = fi.Extension;
            ops.Rewind    = false;
        }

        var key = await repo.Put(MakePut);


        Assert.IsNotNull(key);
        Assert.IsNotEmpty(key);

        void MakeGet(GetOptions ops)
        {
            ops.Key     = key;
            ops.Content = new MemoryStream();
        }

        var exists = await repo.Get( MakeGet );


        Assert.IsTrue(exists);

    }


}

public class RepoTestModule : Module
{


    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.UseRepositoryRemoteClient("https://fabrica.ngrok.io/repository");

    }


}
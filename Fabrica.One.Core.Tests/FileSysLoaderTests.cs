using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.One.Loader;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.One.Core.Tests;

[TestFixture]
public class FileSysLoaderTests : BaseOneTest
{

    [SetUp]
    public void Setup()
    {

        OldDir.Create();


    }

    private DirectoryInfo OldDir { get; } = new (@"e:\fabrica-one\repository\ThisIsOld");
    private DirectoryInfo CurDir { get; } = new (@"e:\fabrica-one\repository\1234567890");


    [Test]
    public async Task Test0200_Clean_Repository()
    {

        var source  = await OneAppliancePlanSourceWithNoChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);

        var loader = new FileSysApplianceLoader();

        await loader.Clean(plan);

        ClassicAssert.IsTrue(CurDir.Exists);
        ClassicAssert.IsFalse(OldDir.Exists);

    }


    [Test]
    public async Task Test0210_Load_Appliance()
    {

        var source  = await OneAppliancePlanSourceWithNoChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];

        var loader = new FileSysApplianceLoader();

        await loader.Load( plan, unit );

        ClassicAssert.IsTrue(CurDir.Exists);

        ClassicAssert.IsTrue(unit.HasLoaded);
        ClassicAssert.IsTrue(unit.RepositoryContent.Length > 0);

    }


    [Test]
    public async Task Test0220_Load_ApplianceWithChecksum()
    {

        var source = await OneAppliancePlanSourceWithGoodChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];

        var loader = new FileSysApplianceLoader();

        await loader.Load(plan, unit);

        ClassicAssert.IsTrue(CurDir.Exists);

        ClassicAssert.IsTrue(unit.HasLoaded);
        ClassicAssert.IsTrue(unit.RepositoryContent.Length > 0);

    }


    [Test]
    public async Task Test0221_Load_ApplianceWithChecksumNoDeploy()
    {

        var source = await OneAppliancePlanSourceWithNoChecksumNoDeploy();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];

        var loader = new FileSysApplianceLoader();

        await loader.Load(plan, unit);

        ClassicAssert.IsTrue(CurDir.Exists);

        ClassicAssert.IsFalse(unit.HasLoaded);
        ClassicAssert.IsFalse(unit.RepositoryContent.Length > 0);

    }



    [Test]
    public async Task Test0230_Load_ApplianceWithBadChecksum()
    {


        var source = await OneApplianceBadChecksumPlanSource();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];

        var loader = new FileSysApplianceLoader();

        var exp = ClassicAssert.ThrowsAsync<Exception>( async ()=>await loader.Load(plan, unit) );


        ClassicAssert.IsFalse(unit.HasLoaded);
        ClassicAssert.IsTrue(unit.RepositoryContent.Length == 0);

    }


    [Test]
    public async Task Test0240_Try_Load_No_Exist_Appliance()
    {

        var source = await OneNoExistAppliancePlanSource();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];

        var loader = new FileSysApplianceLoader();

        ClassicAssert.ThrowsAsync<FileNotFoundException>( async ()=>await loader.Load(plan, unit));

        ClassicAssert.IsTrue(CurDir.Exists);

        ClassicAssert.IsFalse(unit.HasLoaded);
        ClassicAssert.IsFalse(unit.RepositoryContent.Length > 0);

    }


}

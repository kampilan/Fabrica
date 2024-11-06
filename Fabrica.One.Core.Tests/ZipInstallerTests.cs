using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.One.Core.Tests;

[TestFixture]
public class ZipInstallerTests: BaseOneTest
{


    [Test]
    public async Task Test_Installation_Clean()
    {


        var source = await OneAppliancePlanSourceWithNoChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];

        var loader = new ZipInstaller();

        var installDir = new DirectoryInfo(plan.InstallationRoot);
        if( installDir.Exists )
            installDir.Delete(true);

        installDir.Create();
        installDir.CreateSubdirectory("test123");

        await loader.Clean(plan);

        ClassicAssert.IsTrue( installDir.Exists);
        ClassicAssert.IsTrue( installDir.GetDirectories().Length == 0);

    }


    [Test]
    public async Task Test_Install_Appliance()
    {


        var source = await OneAppliancePlanSourceWithNoChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];


        var loader = new FileSysApplianceLoader();
        var installer = new ZipInstaller();

        var installDir = new DirectoryInfo(plan.InstallationRoot);
        if (installDir.Exists)
            installDir.Delete(true);

        installDir.Create();
        installDir.CreateSubdirectory("test123");

        await loader.Clean(plan);
        await installer.Clean(plan);

        await loader.Load(plan, unit);

        ClassicAssert.IsTrue(unit.HasLoaded);

        await installer.Install(plan, unit);

        ClassicAssert.IsTrue(installDir.Exists);
        ClassicAssert.IsTrue(installDir.GetDirectories().Length == 1);


    }


    [Test]
    public async Task Test_Install_Appliance_NoDeploy()
    {


        var source = await OneAppliancePlanSourceWithNoChecksumNoDeploy();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];


        var loader    = new FileSysApplianceLoader();
        var installer = new ZipInstaller();

        var installDir = new DirectoryInfo(plan.InstallationRoot);
        if (installDir.Exists)
            installDir.Delete(true);

        installDir.Create();
        installDir.CreateSubdirectory("test123");

        await loader.Clean(plan);
        await installer.Clean(plan);

        await loader.Load(plan, unit);
        await installer.Install(plan, unit);

        ClassicAssert.IsFalse(unit.HasLoaded);
        ClassicAssert.IsTrue(unit.RepositoryContent.CanRead);
        ClassicAssert.IsFalse(unit.RepositoryContent.Length > 0);

        ClassicAssert.IsFalse(unit.HasInstalled);

        ClassicAssert.IsTrue(installDir.Exists);
        ClassicAssert.IsTrue(installDir.GetDirectories().Length == 0);


    }




    [Test]
    public async Task Test_Install_ApplianceWithChecksum()
    {


        var source = await OneAppliancePlanSourceWithGoodChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];


        var loader = new FileSysApplianceLoader();

        var installer = new ZipInstaller();

        var installDir = new DirectoryInfo(plan.InstallationRoot);
        if (installDir.Exists)
            installDir.Delete(true);

        installDir.Create();
        installDir.CreateSubdirectory("test123");

        await loader.Clean(plan);
        await installer.Clean(plan);

        await loader.Load(plan, unit);
        await installer.Install(plan, unit);

        ClassicAssert.IsTrue(unit.HasLoaded);
        ClassicAssert.IsTrue(unit.HasInstalled);


        ClassicAssert.IsTrue(installDir.Exists);
        ClassicAssert.IsTrue(installDir.GetDirectories().Length == 1);


    }


    [Test]
    public async Task Test_Install_Bogus_Appliance()
    {


        var source = await OneBogusAppliancePlanSource();
        var factory = GetFactory();

        var plan = await factory.Create(source);
        var unit = plan.Deployments[0];


        var loader = new FileSysApplianceLoader();

        var installer = new ZipInstaller();

        var installDir = new DirectoryInfo(plan.InstallationRoot);
        if (installDir.Exists)
            installDir.Delete(true);

        installDir.Create();
        installDir.CreateSubdirectory("test123");

        await loader.Clean(plan);
        await installer.Clean(plan);

        await loader.Load(plan, unit);

        var exp = ClassicAssert.ThrowsAsync<InvalidDataException>( async ()=>await installer.Install(plan, unit) );

        ClassicAssert.IsTrue(unit.HasLoaded);
        ClassicAssert.IsFalse(unit.HasInstalled);


    }










}
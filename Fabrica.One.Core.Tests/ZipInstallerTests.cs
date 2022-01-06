using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.One.Installer;
using Fabrica.One.Loader;
using NUnit.Framework;

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
        
        Assert.IsTrue( installDir.Exists);
        Assert.IsTrue( installDir.GetDirectories().Length == 0);

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

        Assert.IsTrue(unit.HasLoaded);

        await installer.Install(plan, unit);

        Assert.IsTrue(installDir.Exists);
        Assert.IsTrue(installDir.GetDirectories().Length == 1);


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

        Assert.IsFalse(unit.HasLoaded);
        Assert.IsTrue(unit.RepositoryContent.CanRead);
        Assert.IsFalse(unit.RepositoryContent.Length > 0);

        Assert.IsFalse(unit.HasInstalled);

        Assert.IsTrue(installDir.Exists);
        Assert.IsTrue(installDir.GetDirectories().Length == 0);


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

        Assert.IsTrue(unit.HasLoaded);
        Assert.IsTrue(unit.HasInstalled);


        Assert.IsTrue(installDir.Exists);
        Assert.IsTrue(installDir.GetDirectories().Length == 1);


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

        var exp = Assert.ThrowsAsync<InvalidDataException>( async ()=>await installer.Install(plan, unit) );

        Assert.IsTrue(unit.HasLoaded);
        Assert.IsFalse(unit.HasInstalled);


    }










}
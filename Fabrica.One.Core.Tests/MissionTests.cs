using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Configuration;
using NUnit.Framework;

namespace Fabrica.One.Core.Tests;

[TestFixture]

public class MissionTests: BaseOneTest
{

    [Test]
    public async Task Test0600_Load_Module()
    {

        var module = new OneModule
        {
            RepositoryRoot = @"z:\repository",
            InstallationRoot = @"c:\appliances\installations"
        };

        var builder = new ContainerBuilder();
        builder.RegisterModule(module);

        var container = builder.Build();

        Assert.IsNotNull(container);

        await container.DisposeAsync();

    }    


}
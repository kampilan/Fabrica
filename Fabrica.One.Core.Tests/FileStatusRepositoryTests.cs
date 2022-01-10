using System.Linq;
using System.Threading.Tasks;
using Fabrica.One.Repository;
using NUnit.Framework;

namespace Fabrica.One.Core.Tests;

[TestFixture]
public class FileStatusRepositoryTests
{

    [Test]
    public async Task Test01200_Repository_Should_Populate()
    {

        var repo = new FileStatusRepository
        {
            OneRoot = @"e:\fabrica-one"
        };

        var list = (await repo.GetAppliances()).ToList();

        Assert.IsNotNull(list);
        Assert.IsNotEmpty(list);


    }

}
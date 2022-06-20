using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using NUnit.Framework;

namespace Fabrica.Tests.Rql;

[TestFixture]
public class RqlTests
{

    [Test]
    public void Test2300_0100_Parse_Empty()
    {

        var tree = RqlLanguageParser.ToCriteria("()");

        Assert.IsNotNull(tree);



    }

}
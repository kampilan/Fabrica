using System.Collections.Generic;
using System.Linq;
using Auth0.ManagementApi.Clients;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Rql.Serialization;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.Tests.Rql;

[TestFixture]
public class RqlTests
{

    [Test]
    public void Test2300_0100_Parse_Empty()
    {

        var tree = RqlLanguageParser.ToCriteria("()");

        ClassicAssert.IsNotNull(tree);



    }


    [Test]
    public void Test2300_0200_Parse_RqlIn()
    {

        var tree = RqlLanguageParser.ToCriteria("(in(Code,3,4,5))");

        ClassicAssert.IsNotNull(tree);

        var builder = new RqlFilterBuilder<Test>(tree);

        var expr = builder.ToLambda();

        var list = new List<Test>
        {
            new() {Code = 1}, new() {Code = 2}, new() {Code = 3}, new() {Code = 4}, new() {Code = 4}, new() {Code = 6}
        };


        var sub = list.Where(expr).ToList();

        ClassicAssert.IsNotEmpty(sub);
    

    }

    [Test]
    public void Test2300_0300_Parse_RqlInFromCrit()
    {


        var crit = new TestCriteria
        {
            Code = new List<int> {3, 4, 5}
        };

        var builder = RqlFilterBuilder<Test>.Create().Introspect(crit);

        var where = builder.ToSqlWhere();
        var expr = builder.ToLambda();

        var list = new List<Test>
        {
            new() {Code = 1}, new() {Code = 2}, new() {Code = 3}, new() {Code = 4}, new() {Code = 4}, new() {Code = 6}
        };


        var sub = list.Where(expr).ToList();

        ClassicAssert.IsNotEmpty(sub);


    }



}


public class TestCriteria: BaseCriteria
{

    [Criterion(Name = "Code", Operation = RqlOperator.In, Operand = OperandKind.ListOfInt)]
    public ICollection<int> Code { get; set; }

}


public class Test
{
    public int Code { get; set; }
}
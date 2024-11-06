using System;
using Bogus.DataSets;
using Fabrica.Utilities.Types;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.Tests.Utilities;


[TestFixture]
public class ToTimestampStringTests
{


    [Test]
    public void Test0100_TimestampString_Should_Be_28_Length()
    {

        var ts = DateTime.Now.ToTimestampString();

        ClassicAssert.IsNotNull(ts);
        ClassicAssert.IsNotEmpty(ts);
        ClassicAssert.IsTrue(ts.Length == 28);

        var label = "X";
        var x = $"{{label}}";



    }



    [Test]
    public void Test0110_Earlier_TimestampString_Should_Less_Then_Later()
    {

        var tsE = DateTime.Now.AddMinutes(-15).ToTimestampString();

        ClassicAssert.IsNotNull(tsE);
        ClassicAssert.IsNotEmpty(tsE);
        ClassicAssert.IsTrue(tsE.Length == 28);

        var tsL = DateTime.Now.ToTimestampString();

        ClassicAssert.IsNotNull(tsL);
        ClassicAssert.IsNotEmpty(tsL);
        ClassicAssert.IsTrue(tsL.Length == 28);


        var comp1 = string.Compare(tsE, tsL, StringComparison.InvariantCulture);

        ClassicAssert.AreEqual(-1,comp1);


        var comp2 = string.Compare(tsL, tsE, StringComparison.InvariantCulture);

        ClassicAssert.AreEqual(1, comp2);


        var comp3 = string.Compare(tsE, tsE, StringComparison.InvariantCulture);

        ClassicAssert.AreEqual(0, comp3);


    }







}
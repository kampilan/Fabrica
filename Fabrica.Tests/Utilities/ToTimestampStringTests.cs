using System;
using Bogus.DataSets;
using Fabrica.Utilities.Types;
using NUnit.Framework;

namespace Fabrica.Tests.Utilities;


[TestFixture]
public class ToTimestampStringTests
{


    [Test]
    public void Test0100_TimestampString_Should_Be_28_Length()
    {

        var ts = DateTime.Now.ToTimestampString();

        Assert.IsNotNull(ts);
        Assert.IsNotEmpty(ts);
        Assert.IsTrue(ts.Length == 28);

    }



    [Test]
    public void Test0110_Earlier_TimestampString_Should_Less_Then_Later()
    {

        var tsE = DateTime.Now.AddMinutes(-15).ToTimestampString();

        Assert.IsNotNull(tsE);
        Assert.IsNotEmpty(tsE);
        Assert.IsTrue(tsE.Length == 28);

        var tsL = DateTime.Now.ToTimestampString();

        Assert.IsNotNull(tsL);
        Assert.IsNotEmpty(tsL);
        Assert.IsTrue(tsL.Length == 28);


        var comp1 = string.Compare(tsE, tsL, StringComparison.InvariantCulture);

        Assert.AreEqual(-1,comp1);


        var comp2 = string.Compare(tsL, tsE, StringComparison.InvariantCulture);

        Assert.AreEqual(1, comp2);


        var comp3 = string.Compare(tsE, tsE, StringComparison.InvariantCulture);

        Assert.AreEqual(0, comp3);


    }







}
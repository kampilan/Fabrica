using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Fabrica.Models.Support;
using Fabrica.Test.Models.Patch;
using Fabrica.Utilities.Text;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using Fabrica.Watch.Utilities;
using JetBrains.dotMemoryUnit;
using Microsoft.IdentityModel.Abstractions;
using NUnit.Framework;

namespace Fabrica.Tests.Watch;

public class LoggerTests
{

    [OneTimeSetUp]
    public void Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseLocalSwitchSource()
            .WhenNotMatched(Level.Debug, Color.Azure);

        TheSink = new MonitorSink
        {
            Accumulate = true
        };

//        maker.UseBatching();
//        maker.Sinks.AddSink(TheSink);

//        maker.UseQuiet();
        maker.UseRealtime();


        maker.Build();

    }


    [OneTimeTearDown]
    public void Teardown()
    {
        WatchFactoryLocator.Factory.Stop();
    }


    private MonitorSink TheSink { get; set; }

    [Test]
    public async Task Test_8300_0100_ShouldDebugMessage()
    {

        using var logger = this.EnterMethod();

        logger.Debug("Cool");

        var model = new {Test = "Cool", Very = true};

        logger.LogObject(nameof(model), model);

        await Task.Delay(200);

        Assert.AreEqual(3,TheSink.Count);

    }

    [Test]
    public async Task Test_08300_0150_ShouldHandleBadObject()
    {

        using var logger = this.EnterMethod();

        var bad = new BadObject();

        logger.LogObject("Bad", bad);

        Assert.IsTrue(true);

        var ms = new ModelMetaSource();
        ms.AddTypes(typeof(Person).Assembly);
        var mm = new ModelMetaService([ms]);

        await mm.Start();




    }




    [Test]
    public async Task Test_8300_0200_ShouldNotAllocate()
    {

        var count = 1000;


        var sw = new Stopwatch();
        sw.Start();

        for ( int i = 0; i < count; i++ )
        {

            using (var logger = this.EnterMethod())
            {

                logger.Debug("Cool");

                var model = new { Test = "Cool", Very = true };

                logger.LogObject(nameof(model), model);

            }

        }
       

        sw.Stop();

        var total = sw.ElapsedTicks;
        var avg   = sw.ElapsedTicks/count;



        await Console.Out.WriteLineAsync($"        Total: {total} tick(s)");
        await Console.Out.WriteLineAsync($"          Avg: {avg} tick(s)");

        await Console.Out.WriteLineAsync($" Sink Current: {TheSink.Count} event(s)");
        await Console.Out.WriteLineAsync($"   Sink Total: {TheSink.Total} event(s)");

        await Console.Out.WriteLineAsync($"         Pool: {WatchFactoryLocator.Factory.PooledCount} logger(s)");

    }


    [Test]
    public async Task Test_8300_0300_GenerateCorrelation()
    {


        var count = 1_000_000;

        var sw = new Stopwatch();
        sw.Start();

        for (int i = 0; i < count; i++)
        {
            string corr1 = Ulid.NewUlid();
        }


        sw.Stop();

        var total = sw.ElapsedTicks;
        var avg = sw.ElapsedTicks / count;


        await Console.Out.WriteLineAsync($"Total: {total} tick(s)");
        await Console.Out.WriteLineAsync($"  Avg: {avg} tick(s)");


    }

    [Test]
    public void Test_8300_0400_SwitchSource()
    {

        var ss = new SwitchSource()
            .WhenMatched("Tester", "", Level.Debug, Color.AliceBlue)
            .WhenMatched("Tester.X.X", "", Level.Trace, Color.AliceBlue)
            .WhenMatched("Tester.X.XX", "", Level.Debug, Color.AliceBlue)
            .WhenMatched("Tester.X.XXX", "", Level.Info, Color.AliceBlue)
            .WhenMatched("Tester.X.XXXX", "", Level.Warning, Color.AliceBlue)
            .WhenMatched("Tester.X.XXXXX", "", Level.Error, Color.AliceBlue)
            .WhenNotMatched(Level.Quiet, Color.Beige);

        ss.Start();


        var ws1 = ss.Lookup("Tester");
        Assert.IsNotNull(ws1);
        Assert.IsTrue( ws1.Level is Level.Debug);

        var ws2 = ss.Lookup("NotTester");
        Assert.IsNotNull(ws2);
        Assert.IsTrue(ws2.Level is Level.Quiet);

        var ws3 = ss.Lookup("Tester.X.XX");
        Assert.IsNotNull(ws3);
        Assert.IsTrue(ws3.Level is Level.Debug);

        var ws4 = ss.Lookup("Tester.X");
        Assert.IsNotNull(ws4);
        Assert.IsTrue(ws4.Level is Level.Debug);

        var ws5 = ss.Lookup("Tester.X.X");
        Assert.IsNotNull(ws5);
        Assert.IsTrue(ws5.Level is Level.Trace);

        var ws6 = ss.Lookup("Tester.X.XXXX");
        Assert.IsNotNull(ws6);
        Assert.IsTrue(ws6.Level is Level.Warning);

        var ws7 = ss.Lookup("Tester.X.XXXXX");
        Assert.IsNotNull(ws7);
        Assert.IsTrue(ws7.Level is Level.Error);



    }


    [Test]
    public async Task Test_8300_0500_SwitchSource()
    {

        var ss = new SwitchSource()
            .WhenMatched("Tester", "", Level.Debug, Color.AliceBlue)
            .WhenMatched("Tester.X.X", "", Level.Error, Color.AliceBlue)
            .WhenMatched("Tester.X.XX", "", Level.Error, Color.AliceBlue)
            .WhenMatched("Tester.X.XXX", "", Level.Error, Color.AliceBlue)
            .WhenMatched("Tester.X.XXXX", "", Level.Error, Color.AliceBlue)
            .WhenMatched("Tester.X.XXXXX", "", Level.Error, Color.AliceBlue)
            .WhenMatched("Tester.X.XXXXXX", "", Level.Error, Color.AliceBlue)
            .WhenNotMatched(Level.Quiet, Color.Beige);

        ss.Start();


        var count = 1_000_000;

        var c1 = "Tester";
        var c2 = "NotTester";


        var sw = new Stopwatch();
        sw.Start();

        for (int i = 0; i < count; i++)
        {

            var m = i % 2;
            if (i % 2 == 0)
            {
                var ws = ss.Lookup(c1);
            }
            else
            {
                var ws = ss.Lookup(c2);
            }

        }


        sw.Stop();

        var total = sw.ElapsedTicks;
        var avg = sw.ElapsedTicks / count;


        await Console.Out.WriteLineAsync($"Total: {total} tick(s)");
        await Console.Out.WriteLineAsync($"  Avg: {avg} tick(s)");


    }



}
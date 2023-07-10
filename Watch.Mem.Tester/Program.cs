using System.Diagnostics;
using System.Drawing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Fabrica.Watch;
using Fabrica.Watch.Sink;

var summary = BenchmarkRunner.Run<WatchBenchmark>();

void Delay(TimeSpan dur)
{

    var ticks = dur.Ticks;
    var start = Stopwatch.GetTimestamp();
    long now;
    do
    {
        now = Stopwatch.GetTimestamp();

    } while ( (now-start) < ticks );

}


[MemoryDiagnoser]
public class WatchBenchmark
{

    const string Category = "Fabrica.Logging.Test";

    private static object TheModel { get; } = new { Test = 1, Very = true };
    private static string MethodName { get; } = "Loop";
    private static string VarName { get; } = "X";
    private static string ModelName { get; } = "TheModel";

    private static ILogger QuietLogger { get; } = new QuietLogger();

    [GlobalSetup]
    public void Setup()
    {

        var theSink = new MonitorSink
        {
            Accumulate = false
        };


        var maker = new WatchFactoryBuilder();
        maker.UseLocalSwitchSource()
            .WhenMatched(Category, "Test", Level.Info, Color.Bisque)
//            .WhenMatched("ZZZZZ", "Test", Level.Debug, Color.Bisque)
            .WhenNotMatched(Level.Quiet);

        maker.UseBatching(1000, TimeSpan.FromMilliseconds(50));
        maker.Sinks.AddSink(theSink);

        //maker.UseQuiet();

        maker.Build();

    }

    [GlobalCleanup]
    public void Cleanup()
    {
        WatchFactoryLocator.Factory.Stop();
    }

    [Benchmark]
    public void QuietBenchmark()
    {

        using (var logger = WatchFactoryLocator.Factory.GetLogger(Category))
        {

            logger.EnterMethod(MethodName);

            logger.Debug(VarName);

            logger.Inspect(VarName, 1);

            logger.LogObject(ModelName, TheModel);

        }
   

    }

}
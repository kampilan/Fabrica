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

    private static MonitorSink TheSink { get; } = new (){Accumulate = false};

    [GlobalSetup]
    public void Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseLocalSwitchSource()
//            .WhenMatched(Category, "Test", Level.Info, Color.Bisque)
            .WhenMatched("WatchBenchmark", "Test", Level.Debug, Color.Bisque)
            .WhenNotMatched(Level.Quiet);

        maker.UseBatching(1000, TimeSpan.FromMilliseconds(50));
        maker.Sinks.AddSink(TheSink);

//        maker.UseQuiet();

        maker.Build();

    }

    [GlobalCleanup]
    public async Task Cleanup()
    {

        WatchFactoryLocator.Factory.Stop();

        await Task.Delay(2000);

        await using var fs = new FileStream("e:/logs/output.txt", FileMode.Create, FileAccess.Write);
        await using var sw = new StreamWriter(fs);
        await sw.WriteLineAsync($"Sink Count: ({TheSink.Total})");
       

    }

    [Benchmark]
    public void QuietBenchmark()
    {


//        using var logger = this.EnterMethodSlim( Category );
        using var logger = this.EnterMethod();

        //        logger.EnterMethod(MethodName);

//        logger.Debug(VarName);

//        logger.Inspect(VarName, 1);

//        logger.LogObject(ModelName, TheModel);
   

    }

}
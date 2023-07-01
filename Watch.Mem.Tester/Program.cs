using System.Diagnostics;
using System.Drawing;
using Fabrica.Watch;
using Fabrica.Watch.Sink;

const string Category = "Fabrica.Logging.test";

var theSink = new MonitorSink
{
    Accumulate = false
};

var maker = new WatchFactoryBuilder();
maker.UseLocalSwitchSource()
    .WhenMatched(Category, "Test", Level.Debug, Color.Bisque )
    .WhenNotMatched( Level.Debug, Color.Azure);

maker.UseBatching( 1000, TimeSpan.FromMilliseconds(50) );
maker.Sinks.AddSink(theSink);

//maker.UseQuiet();

maker.Build();



while (true)
{

    Console.Out.WriteLine("Press any key to start");
    var key = Console.ReadKey();
    if (key.KeyChar == 'x')
        break;


    var count = 100_000;
    var x = "Cool";

    var sw = new Stopwatch();

    sw.Start();
    for (int i = 0; i < count; i++)
    {

        using (var logger = WatchFactoryLocator.Factory.GetLogger(Category))
        {

            logger.EnterMethod("Loop");

            logger.Debug(x);

            logger.Inspect(nameof(i), i);

            var model = new { Test = x, Very = true };

            logger.LogObject(nameof(model), model);

        }

        Delay(TimeSpan.FromMilliseconds(1));

    }
    sw.Stop();

    await Task.Delay(200);


    var total = sw.ElapsedTicks;
    var avg = sw.ElapsedTicks / count;

    Console.Clear();

    await Console.Out.WriteLineAsync($"     Duration: {sw.Elapsed:g}");
    await Console.Out.WriteLineAsync($"        Total: {total} tick(s)");
    await Console.Out.WriteLineAsync($"          Avg: {avg} tick(s)");

    await Console.Out.WriteLineAsync($" Sink Current: {theSink.Count} event(s)");
    await Console.Out.WriteLineAsync($"   Sink Total: {theSink.Total} event(s)");

    await Console.Out.WriteLineAsync($"         Pool: {WatchFactoryLocator.Factory.PooledCount} logger(s)");

    GC.Collect();


}

WatchFactoryLocator.Factory.Stop();

Console.Out.WriteLine("Watch Factory stopped");
Console.ReadKey();


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



using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using Fabrica.One.Persistence;
using Fabrica.One.Persistence.Work;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Fabrica.Work.Processor.Parsers;
using NCrontab.Advanced;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Fabrica.Tests.Work;


[TestFixture]
public class WorkTests
{

    [OneTimeSetUp]
    public async Task StartUp()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.Aqua);

        maker.Build();

        var builder = new ContainerBuilder();

        builder.RegisterModule<WorkTestModule>();

        Container = await builder.BuildAndStart();

    }

    public void TearDown()
    {

        Container.Dispose();

        WatchFactoryLocator.Factory.Stop();
            
    }

    private IContainer Container { get; set; }

    [Test]
    public void Test_0500_0100_LoadS3Event()
    {

        using( var stream = new FileStream("Z:/Kampilan/s3-event.json", FileMode.Open, FileAccess.Read) )
        using( var reader = new StreamReader(stream) )
        using( var jreader = new JsonTextReader(reader))
        {

            var parser = new JsonSerializer();
            var s3Event = parser.Deserialize<S3Event>( jreader );

            Assert.IsNotNull(s3Event);

        }

    }


    [Test]
    public async Task Test_0500_0200_ParseS3Event()
    {

        using (var stream = new FileStream("Z:/Kampilan/s3-event.json", FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(stream))
        using (var jreader = new JsonTextReader(reader))
        {

            var body = await reader.ReadToEndAsync();

            var parser = new S3EventMessageBodyParser();

            var request = await parser.Parse(body);

            Assert.IsNotNull(request);
            Assert.IsTrue(request.ok);
            Assert.IsNotNull(request.request);

        }

    }


    [Test]
    public async Task Test_0500_0300_ParseBadS3Event()
    {

        using (var stream = new FileStream("Z:/Kampilan/s3-event-bad.json", FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(stream))
        using (var jreader = new JsonTextReader(reader))
        {

            var body = await reader.ReadToEndAsync();

            var parser = new S3EventMessageBodyParser();

            var request = await parser.Parse(body);

            Assert.IsNotNull(request);
            Assert.IsFalse(request.ok);
            Assert.IsNull(request.request);

        }

    }

    [Test]
    public async Task Test_0500_0400_ParseEmptyS3Event()
    {

        using (var stream = new FileStream("Z:/Kampilan/s3-event-empty.json", FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(stream))
        using (var jreader = new JsonTextReader(reader))
        {

            var body = await reader.ReadToEndAsync();

            var parser = new S3EventMessageBodyParser();

            var request = await parser.Parse(body);

            Assert.IsNotNull(request);
            Assert.IsFalse(request.ok);
            Assert.IsNull(request.request);


        }

    }


    [Test]
    public async Task Test_0500_0500_GoodTopic()
    {

        using (var scope = Container.BeginLifetimeScope())
        {

            var repository = scope.Resolve<WorkRepository>();
            var topic = await repository.GetTopic("test-files");

            Assert.IsNotNull(topic);

        }


    }


    [Test]
    public async Task Test_0500_0510_BadTopic()
    {

        using (var scope = Container.BeginLifetimeScope())
        {

            var repository = scope.Resolve<WorkRepository>();
            var topic = await repository.GetTopic("inbound-test-bad");

            Assert.IsNull(topic);

        }


    }

    [Test]
    public void Test_0500_0600_CronParse()
    {

        var inst1 = CrontabSchedule.Parse("0 0 14,L * ?");

        var next1 = inst1.GetNextOccurrence(new DateTime(2022, 8, 1, 0, 0, 0));
        next1 = inst1.GetNextOccurrence(next1);


        var inst2 = CrontabSchedule.Parse("0 0 1,15 * ?");

        var next2 = inst2.GetNextOccurrence(new DateTime(2022, 8, 1, 0, 0, 0));

        var pt = inst1.GetNextOccurrence(next2);
        
        
        next2 = inst2.GetNextOccurrence(next2);




        var inst3 = CrontabSchedule.Parse("0 0 * * 5L");

        var next3 = inst3.GetNextOccurrence(new DateTime(2022, 8, 1, 0, 0, 0));
        next3 = inst3.GetNextOccurrence(next3);


        var inst4 = CrontabSchedule.Parse("0 0 */7 * ?");

        var next4 = inst4.GetNextOccurrence(new DateTime(2022, 8, 3, 0, 0, 0));
        next4 = inst4.GetNextOccurrence(next4);
        next4 = inst4.GetNextOccurrence(next4);
        next4 = inst4.GetNextOccurrence(next4);
        next4 = inst4.GetNextOccurrence(next4);
        next4 = inst4.GetNextOccurrence(next4);



    }




}

public class WorkTestModule : Module
{

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.UseOnePersitence("mongodb://mongodb.fabricatio.io:27017");

    }

}

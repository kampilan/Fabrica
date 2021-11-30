
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Fabrica.Work.Processor.Parsers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Fabrica.Tests.Work
{


    [TestFixture]
    public class WorkTests
    {

        [OneTimeSetUp]
        public void StartUp()
        {

            var maker = new WatchFactoryBuilder();
            maker.UseRealtime();
            maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.Aqua);

            maker.Build();

        }

        public void TearDown()
        {

            WatchFactoryLocator.Factory.Stop();
            
        }

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

                var body = reader.ReadToEnd();

                var parser = new S3EventMessageBodyParser();

                var request = await parser.Parse(body);

                Assert.IsNotNull(request);

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

                Assert.IsNull(request);

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

                Assert.IsNull(request);

            }

        }



    }

}

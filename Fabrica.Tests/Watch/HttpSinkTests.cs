using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Watch;
using NUnit.Framework;

namespace Fabrica.Tests.Watch
{

    [TestFixture]
    public class HttpSinkTests 
    {

        [OneTimeSetUp]    
        public void Setup()
        {
            var maker = new WatchFactoryBuilder();
            maker.UseConsoleSink();
            //maker.UseHttpSink("https://kampilan.ngrok.io/watch/", "After.Client" );
            maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.Azure);

            maker.Build();

        }

        [OneTimeTearDown]
        public void TearDown()
        {
            WatchFactoryLocator.Factory.Stop();
        }

        [TestCase]
        public void Test_0101_0001_LogTest()
        {

            var logger = WatchFactoryLocator.Factory.GetLogger("After.Test.Controllers");
            
            logger.Debug("This is a test");

        }





    }
}

/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using JetBrains.Annotations;
using MongoDB.Driver;

namespace Fabrica.Watch.Mongo.Switches
{


    public class MongoSwitchSource: SwitchSource
    {


        public string ServerUri { get; set; } = "";
        public MongoSwitchSource WithServerUri([NotNull] string uri )
        {

            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));

            ServerUri = uri;
            return this;
        }


        public string DomainName { get; set; } = "";
        public MongoSwitchSource WithDomainName([NotNull] string domainName )
        {

            if (string.IsNullOrWhiteSpace(domainName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(domainName));

            DomainName = domainName;
            return this;
        }



        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(15);
        public MongoSwitchSource WithPollingInterval(TimeSpan interval)
        {
            PollingInterval = interval;
            return this;
        }


        public TimeSpan WaitForStopInterval { get; set; } = TimeSpan.FromSeconds(5);
        public MongoSwitchSource WithWaitForStopInterval(TimeSpan interval)
        {
            WaitForStopInterval = interval;
            return this;
        }



        private IMongoCollection<DomainEntity> DomainCollection { get; set; }
        private IMongoCollection<SwitchEntity> SwitchCollection { get; set; }


        private EventWaitHandle MustStop { get; } = new EventWaitHandle(false, EventResetMode.ManualReset);
        private EventWaitHandle Stopped { get; } = new EventWaitHandle(false, EventResetMode.ManualReset);

        private ConsoleEventSink DebugSink { get; } = new ConsoleEventSink();


        private bool Started { get; set; }
        public override void Start()
        {

            try
            {

                if (Started)
                    return;
            
                var client   = new MongoClient(ServerUri);
                var database = client.GetDatabase("fabrica_watch");

                DomainCollection = database.GetCollection<DomainEntity>("domains");
                SwitchCollection = database.GetCollection<SwitchEntity>("switches");

                Fetch();


                var task = new Task(_process);
                task.Start();

                Started = true;


            }
            catch (Exception cause)
            {

                var le = new LogEvent
                {
                    Category = GetType().FullName,
                    Level    = Level.Debug,
                    Title    = cause.Message,
                    Payload  = cause.StackTrace
                };

                DebugSink.Accept( le );
                
            }


        }

        public override void Stop()
        {

            MustStop.Set();

            Stopped.WaitOne(WaitForStopInterval);

            base.Stop();

        }

        private void _process()
        {

            while (!MustStop.WaitOne(PollingInterval))
                Fetch();

            Stopped.Set();

        }


        protected void Fetch()
        {

            try
            {


                var domCursor = DomainCollection.Find(d => d.Name == DomainName);
                var domain    = domCursor.SingleOrDefault();

                var switchCursor = SwitchCollection.Find(s=>s.DomainUid == domain.Uid);
                var switchList   = switchCursor.ToList();


                var switches = new List<SwitchDef>();

                foreach( var se in switchList )
                {


                    if ( !(Enum.TryParse( se.Level, true, out Level lv)) )
                        lv = Level.Warning;


                    var color = Color.White;
                    try
                    {
                        color = Color.FromName(se.Color);
                    }
                    catch
                    {
                        // ignore
                    }


                    var sw = new SwitchDef
                    {
                        Pattern      = se.Pattern,
                        Tag          = se.Tag,
                        FilterType   = se.FilterType,
                        FilterTarget = se.FilterTarget,
                        Level        = lv,
                        Color        = color
                    };

                    switches.Add(sw);

                }



                Update( switches );



            }
            catch (Exception cause)
            {

                var le = new LogEvent
                {
                    Category = GetType().FullName,
                    Level    = Level.Debug,
                    Title    = cause.Message,
                    Payload  = cause.StackTrace
                };

                DebugSink.Accept( le );
                
            }


        }


    }



}

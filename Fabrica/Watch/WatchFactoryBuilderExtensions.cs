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

using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using JetBrains.Annotations;

namespace Fabrica.Watch
{


    public static class WatchFactoryBuilderExtensions
    {

        [NotNull]
        public static ConsoleEventSink UseConsoleSink( [NotNull] this WatchFactoryBuilder builder )
        {
            var console = new ConsoleEventSink();
            builder.Sinks.AddSink( console );
            return console;
        }

        [NotNull]
        public static QueueEventSink UseQueueSink([NotNull] this WatchFactoryBuilder builder, int maxQueueLength=1000)
        {

            var sink = new QueueEventSink
            {
                MaximumCount = maxQueueLength

            };

            builder.Sinks.AddSink(sink);

            return sink;

        }


        public static HttpEventSink UseHttpSink([NotNull] this WatchFactoryBuilder builder, [NotNull] string uri, [NotNull] string domain )
        {

            var sink = new HttpEventSink
            {
                WatchEndpoint = uri,
                Domain        = domain
            };

            builder.Sinks.AddSink( sink );

            return sink;

        }


        [NotNull]
        public static SwitchSource UseLocalSwitchSource( [NotNull] this WatchFactoryBuilder builder )
        {
            var local = new SwitchSource();
            builder.Source = local;
            return local;
        }

    }

}

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
using Fabrica.Utilities.Drawing;
using JetBrains.Annotations;

namespace Fabrica.Watch
{


    public static class WatchFactoryLocator
    {

        static WatchFactoryLocator()
        {

            var builder = new WatchFactoryBuilder();

            builder.UseConsoleSink();
            builder.UseLocalSwitchSource()
                .WhenNotMatched(Level.Warning, Color.Green);

            builder.Build();

        }

        public static void SetFactory( [NotNull] IWatchFactory factory )
        {

            if (factory == null) throw new ArgumentNullException(nameof(factory));


            var previous = Factory;

            factory.Start();
            Factory = factory;

            previous?.Stop();

        }


        public static IWatchFactory Factory { get; private set; }


    }


}

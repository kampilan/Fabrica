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

namespace Fabrica.Watch;

public class WatchFactoryBuilder
{


    public static WatchFactoryBuilder Create()
    {
        return new WatchFactoryBuilder();
    }

    public int InitialPoolSize { get; set; } = 50;
    public int MaxPoolSize { get; set; } = 500;


    public CompositeSink Sinks { get; } = new ();

    public ISwitchSource Source { get; set; } = new SwitchSource();

    public IList<object> Infrastructure { get; } = new List<object>();


    private bool Quiet { get; set; }

    public WatchFactoryBuilder UseQuiet()
    {
        Quiet = true;
        return this;
    }



    private bool WillUseBatching { get; set; }
    private int BatchSize { get; set; } = 10;
    private TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(50);
    
    public WatchFactoryBuilder UseBatching( int batchSize=10, TimeSpan pollingInterval=default ) 
    {

        WillUseBatching = true;

        BatchSize = batchSize;

        if ( pollingInterval != default)
            PollingInterval = pollingInterval;

        return this;

    }



    private IEventSink _buildSink()
    {

        if( WillUseBatching )
        {
            var sinks = new BatchEventSink(Sinks)
            {
                BatchSize = BatchSize,
                PollingInterval = PollingInterval
            };
            return sinks;
        }
            
        return Sinks;

    }




    public void Build<TFactory>() where TFactory : class, IWatchFactory, new()
    {

        var sink = _buildSink();

        var factory = new TFactory();

        foreach( var i in Infrastructure)
            factory.AddInfrastructure(i);

        factory.Configure( Source, sink, Quiet );

        WatchFactoryLocator.SetFactory( factory );

    }

    public void Build<TFactory>( Func<TFactory> builder ) where TFactory : class, IWatchFactory
    {

        var sink = _buildSink();

        var factory = builder();

        foreach (var i in Infrastructure)
            factory.AddInfrastructure(i);

        factory.Configure(Source, sink, Quiet );

        WatchFactoryLocator.SetFactory( factory );

    }

    public void Build()
    {

        var sink = _buildSink();

        var factory = new WatchFactory(InitialPoolSize, MaxPoolSize);

        foreach (var i in Infrastructure)
            factory.AddInfrastructure(i);

        factory.Configure( Source, sink, Quiet );

        WatchFactoryLocator.SetFactory(factory);

    }



    public IWatchFactory BuildNoSet()
    {

        var sink = _buildSink();

        var factory = new WatchFactory(InitialPoolSize);

        foreach (var i in Infrastructure)
            factory.AddInfrastructure(i);

        factory.Configure(Source, sink, Quiet);

        return factory;

    }





}
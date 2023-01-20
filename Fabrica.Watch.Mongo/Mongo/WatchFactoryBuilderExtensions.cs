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


// ReSharper disable UnusedMember.Global

using Fabrica.Watch.Mongo.Sink;
using Fabrica.Watch.Mongo.Switches;


namespace Fabrica.Watch.Mongo;

public static class WatchFactoryBuilderExtensions
{


    public static WatchFactoryBuilder UseMongo( this WatchFactoryBuilder builder, IWatchMongoModule module )
    {

        builder.UseMongoSink( module );
        builder.UseWatchSwitchSource( module );

        return builder;

    }


    public static WatchFactoryBuilder UseMongo( this WatchFactoryBuilder builder, string serverUri, string domainName, bool useBatching = true )
    {

        if( useBatching )
            builder.UseBatching();

        builder.UseMongoSink( serverUri, domainName );
        builder.UseWatchSwitchSource( serverUri, domainName );

        return builder;

    }



    public static WatchFactoryBuilder UseMongoSink( this WatchFactoryBuilder builder, IWatchMongoModule module )
    {


        var mongoSink = new MongoEventSink
        {
            ServerUri  = module.WatchEventStoreUri,
            DomainName = module.WatchDomainName
        };

        builder.Sinks.AddSink(mongoSink);


        return builder;

    }


    public static WatchFactoryBuilder UseMongoSink(this WatchFactoryBuilder builder, string serverUri, string domainName )
    {

        var mongoSink = new MongoEventSink
        {
            ServerUri  = serverUri,
            DomainName = domainName
        };

        builder.Sinks.AddSink(mongoSink);


        return builder;

    }


    public static WatchFactoryBuilder UseWatchSwitchSource( this WatchFactoryBuilder builder, IWatchMongoModule module )
    {

        var source = new MongoSwitchSource
        {
            DomainName          = module.WatchDomainName,
            ServerUri           = module.WatchEventStoreUri,
            PollingInterval     = TimeSpan.FromSeconds(module.WatchPollingDurationSecs),
            WaitForStopInterval = TimeSpan.FromSeconds(30)
        };


        builder.Source = source;

        return builder;

    }



    public static WatchFactoryBuilder UseWatchSwitchSource( this WatchFactoryBuilder builder, string serverUri, string domainName )
    {

        var source = new MongoSwitchSource
        {
            ServerUri  = serverUri,
            DomainName = domainName
        };

        builder.Source = source;

        return builder;

    }



}
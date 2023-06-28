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

using System.Collections.Concurrent;
using Fabrica.Watch.Pool;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;

namespace Fabrica.Watch;

public class WatchFactory : IWatchFactory
{

    private static readonly ILogger Silencer = new QuietLogger();


    public WatchFactory( int initialPoolSize=50, int maxPoolSize=500 )
    {

        ForObject = new NewtonsoftWatchObjectSerializer();
        ForException = new TextExceptionSerializer();

        InitialPoolSize = initialPoolSize;
        MaxPoolSize     = maxPoolSize;

    }


    private IWatchObjectSerializer ForObject { get; set; }
    private IWatchExceptionSerializer ForException { get; set; }



    private int InitialPoolSize { get; set; }
    private int MaxPoolSize { get; set; }
    private Pool<Logger> Pool { get; set; } = null!;


    public bool Quiet { get; set; }

    public ISwitchSource Switches { get; set; } = null!;
    public IEventSink Sink { get; set; } = null!;

    public IEventSink? GetSink<T>() where T : class, IEventSink
    {

        IEventSink? snk = null;
        switch (Sink)
        {
            case CompositeSink cs:
                snk = cs.InnerSinks.FirstOrDefault(s => s is T);
                break;
            case T _:
                snk = Sink;
                break;
        }

        return snk;

    }

    private readonly ConcurrentBag<object> _infrastructure = new ();
    public TType? GetInfrastructure<TType>() where TType: class
    {
        var item = _infrastructure.FirstOrDefault(i => i is TType);
        return item as TType;
    }

    public void AddInfrastructure( object item )
    {
        _infrastructure.Add(item);
    }


    public virtual void Configure( ISwitchSource switches, IEventSink sink, bool quiet=false )
    {

        Quiet = quiet;

        Switches = switches ?? throw new ArgumentNullException(nameof(switches));
        Sink     = sink ?? throw new ArgumentNullException(nameof(sink));

        if( Quiet )
        {
            InitialPoolSize = 0;
            MaxPoolSize = 0;
        }


    }


    public virtual void Start()
    {


        Pool = new Pool<Logger>(() => new Logger(l => Pool.Return(l)), MaxPoolSize);

        for (var i = 0; i < InitialPoolSize; i++)
            Pool.Return(new Logger(Pool.Return));


        Switches.Start();

        Sink.Start();

    }


    public virtual void Stop()
    {

        try
        {
            Switches.Stop();
            Switches = null!;
        }
        catch
        {
            //ignored
        }

        try
        {
            Sink.Stop();
            Sink = null!;
        }
        catch
        {
            //ignored
        }

        try
        {
            Pool.Clear();
            Pool = null!;
        }
        catch
        {
            // ignored
        }


        try
        {

            foreach( var item in _infrastructure )
            {
                if( item is IDisposable disp )
                    disp.Dispose();
            }

        }
        catch
        {
            //ignored
        }


    }

    public int PooledCount => Pool.Count;


    public virtual ILogger GetLogger( string category, bool retroOn=true )
    {

        // Overall Quiet
        if (Quiet)
            return Silencer;


        // Get Switch for given Category
        var sw = Switches.Lookup( category );


        // Return the silencer if switch level is quiet
        if (sw.Level is Level.Quiet)
            return Silencer;


        // Acquire a new logger from the pool
        var logger = Pool.Acquire(0);



        // Config the acquired logger
        logger.Config(Sink, retroOn, "", "", sw.Tag, category, "", sw.Level, sw.Color);


        // **************************************
        return logger;

    }


    public virtual ILogger GetLogger<T>( bool retroOn = true )
    {

        var category = typeof(T).FullName??"";
        var logger   = GetLogger( category, retroOn );

        return logger;

    }



    public virtual ILogger GetLogger( Type type, bool retroOn = true)
    {

        var category = type.FullName??"";
        var logger   = GetLogger( category, retroOn );

        return logger;

    }



    public ILogger GetLogger( LoggerRequest request, bool retroOn = true )
    {

        if (request == null) throw new ArgumentNullException(nameof(request));


        // Overall Quiet
        if (Quiet)
            return Silencer;


        // Check for the special case of the diagnostic probe
        ISwitch sw;
        if ( request.Debug )
            sw = new Switch {Level = request.Level, Color = request.Color, Pattern = "", Tag = "Diagnostics"};
        else
            sw = Switches.Lookup( request.Category );



        // Return the silencer if switch level is quiet
        if ( sw.Level is Level.Quiet )
            return Silencer;


        // Acquire a new logger from the pool
        var logger = Pool.Acquire(0);


        // Config the acquired logger
        logger.Config( Sink, retroOn, request.Tenant, request.Subject, sw.Tag, request.Category, request.CorrelationId, sw.Level, sw.Color );


        // ************************************************************
        return logger;


    }


    public void Enrich( ILogEvent logEvent )
    {

        if( !string.IsNullOrWhiteSpace(logEvent.Payload) )
            return;

        if( logEvent.Error is not null )
        {
            var (type, source) = ForException.Serialize(logEvent.Error, logEvent.ErrorContext, logEvent.Retro);
            logEvent.Type = type;
            logEvent.Payload = source;
        }
        else if( logEvent.Object is not null )
        {
            var (type, source) = ForObject.Serialize(logEvent.Object);
            logEvent.Type = type;
            logEvent.Payload = source;
        }

    }

    public void Encode( ILogEvent logEvent )
    {

        if( !string.IsNullOrWhiteSpace(logEvent.Payload) && string.IsNullOrWhiteSpace(logEvent.Base64) )
            logEvent.Base64 = WatchPayloadEncoder.Encode(logEvent.Payload);

    }


}
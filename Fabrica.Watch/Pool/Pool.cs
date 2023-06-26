﻿/*
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

namespace Fabrica.Watch.Pool;

public class Pool<TPooled>: IDisposable where TPooled : class
{


    public Pool( Func<TPooled> factory, int maxSize=int.MaxValue )
    {

        Queue          = new ConcurrentQueue<TPooled>();
        AvailableEvent = new AutoResetEvent(false);

        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        MaxSize = maxSize;

    }


    private Func<TPooled> Factory { get; }

    private int MaxSize { get; }

    private ConcurrentQueue<TPooled> Queue { get; }

    private AutoResetEvent AvailableEvent { get; }


    public int Count => Queue.Count;

    public void Clear() => Queue.Clear();

    public TPooled Acquire( int waitDuration=int.MaxValue )
    {

        TPooled? item;
        do
        {
            if( !Queue.TryDequeue( out item ) && !AvailableEvent.WaitOne( waitDuration ) )
                item = Factory();
        }
        while (null == item);

        return item;

    }

    public void Return( TPooled item )
    {

        if (item == null) throw new ArgumentNullException(nameof(item));

        var size=0;
        if( Interlocked.CompareExchange(ref size, Queue.Count, MaxSize) == 0 )
        {
            Queue.Enqueue( item );
            AvailableEvent.Set();
        }


    }

    public void Dispose()
    {

        AvailableEvent.Dispose();
        Queue.Clear();

    }
}
using System.Collections.Concurrent;
using Autofac;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Search;

public class SearchIndexManager<TIndex>: ISearchIndexState<TIndex>, ISearchIndexBuildRequestHandler<TIndex>, IRequiresStart, IDisposable where TIndex : class
{


    public SearchIndexManager( ILifetimeScope rootScope )
    {
        RootScope = rootScope;
    }

    private ILifetimeScope RootScope { get; }



    private readonly ReaderWriterLockSlim _lock = new ();
    private Memory<byte> _source = Array.Empty<byte>();


    public async Task CheckState()
    {

        await Load();

        if( _source.IsEmpty )
            await _build();

    }


    public async Task ReadState( Func<Memory<byte>,Task> reader )
    {


        try
        {

            _lock.EnterReadLock();


            await reader.Invoke(_source);

        }
        finally
        {
            _lock.ExitReadLock();            
        }

    }


    public async Task SetState( Memory<byte> source )
    {

        UpdateSource(source);

        await Save(_source);

    }


    protected void UpdateSource(Memory<byte> source)
    {

        try
        {

            _lock.EnterWriteLock();

            _source = source.ToArray();

        }
        finally
        {
            _lock.ExitWriteLock();
        }


    }


    protected virtual Task Load()
    {
        return Task.CompletedTask;
    }

    protected virtual Task Save( Memory<byte> source )
    {
        return Task.CompletedTask;
    }


    private class BuildRequest
    {
    }


    public Task RequestBuild()
    {

        Requests.Enqueue(new BuildRequest());

        return Task.CompletedTask;

    }


    private ConcurrentQueue<BuildRequest> Requests { get; } = new();
    private ManualResetEvent MustStop { get; } = new(false);
    private ManualResetEvent Stopped { get; } = new(false);

    public Task Start()
    {

        using var logger = this.EnterMethod();

        Task.Run(Run);

        return Task.CompletedTask;

    }


    public void Dispose()
    {

        using var logger = this.EnterMethod();


        MustStop.Set();
        Stopped.WaitOne(5000);

        MustStop.Dispose();
        Stopped.Dispose();

    }


    private async Task Run()
    {


        // *****************************************************************
        using var loggerEn = this.GetLogger();

        loggerEn.Debug("Attempting to Initialize provider");

        await _init();

        loggerEn.Debug("Enter Run");
        loggerEn.Dispose();


        while( !MustStop.WaitOne(TimeSpan.FromMilliseconds(1000)) )
        {

            if( Requests.TryDequeue(out _) )
            {

                try
                {

                    await _build();

                }
                catch (Exception cause)
                {
                    using var loggerEr = this.GetLogger();
                    loggerEr.Error( cause, "BuildIndex failed.");
                    loggerEr.Dispose();
                }

                Requests.Clear();

            }

        }


        // *****************************************************************
        using var loggerEx = this.GetLogger();
        loggerEx.Debug("Exit Run");
        loggerEx.Dispose();


        Stopped.Set();


    }


    private async Task _init()
    {

        await using var scope = RootScope.BeginLifetimeScope();

        var provider = scope.Resolve<ISearchProvider<TIndex>>();

        await provider.Initialize();

    }


    private async Task _build()
    {

        await using var scope = RootScope.BeginLifetimeScope();

        var provider = scope.Resolve<ISearchProvider<TIndex>>();

        await provider.BuildIndex();

    }


}
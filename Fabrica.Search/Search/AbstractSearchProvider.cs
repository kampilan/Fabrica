using Fabrica.Utilities.Container;
using Lifti;
using Lifti.Serialization.Binary;
using System.Collections.Concurrent;
using Fabrica.Watch;

namespace Fabrica.Search;

// ReSharper disable once UnusedTypeParameter
public abstract class AbstractSearchProvider<TDocument,TIndex>: CorrelatedObject, ISearchProvider<TIndex>, IRequiresStart, IDisposable where TDocument : class where TIndex: class
{

    private class InputKeySerializer : IKeySerializer<InputKey>
    {

        public void Write(BinaryWriter writer, InputKey key)
        {
            writer.Write(key.Entity);
            writer.Write(key.Id);
        }

        public InputKey Read(BinaryReader reader)
        {

            var entity = reader.ReadString();
            var id = reader.ReadInt64();

            return new InputKey(entity, id);

        }

    }

    protected AbstractSearchProvider( ICorrelation correlation): base(correlation)
    {
    }

    protected abstract Task<IEnumerable<TDocument>> GetInputs();

    protected abstract FullTextIndex<InputKey> CreateIndexDefinition();


    private readonly ReaderWriterLockSlim _lock = new ();
    private FullTextIndex<InputKey>? _currentIndex;
    protected FullTextIndex<InputKey>? CurrentIndex
    {

        get => _currentIndex;
        set
        {

            try
            {

                _lock.EnterWriteLock();

                _currentIndex?.Dispose();
                _currentIndex = value;

            }
            finally
            {
                _lock.ExitWriteLock();
            }

        }

    }


    protected virtual Task<bool> Load()
    {
        return Task.FromResult(false);
    }

    protected virtual Task Save( MemoryStream stream )
    {
        return Task.CompletedTask;
    }


    protected async Task BuildIndex()
    {

        using var logger = EnterMethod();

        

        // *****************************************************************
        logger.Debug("Attempting to create Index definition");
        var index = CreateIndexDefinition();



        // *****************************************************************
        logger.Debug("Attempting to get Input documents");
        var inputs = await GetInputs();

        await index.AddRangeAsync(inputs);



        // *****************************************************************
        logger.Debug("Attempting to serialize new index");
        using var stream = new MemoryStream();
        var serializer = new BinarySerializer<InputKey>( new InputKeySerializer() );
        await serializer.SerializeAsync(index, stream, false);

        stream.Seek( 0, SeekOrigin.Begin );
        await Save(stream);



        // *****************************************************************
        logger.Debug("Attempting to set CurrentIndex to new index");
        CurrentIndex = index;


    }

    protected async Task UpdateIndex( MemoryStream stream )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to create Index definition");
        var index = CreateIndexDefinition();


        // *****************************************************************
        logger.Debug("Attempting to deserialize updated index");
        stream.Seek(0, SeekOrigin.Begin);
        var serializer = new BinarySerializer<InputKey>(new InputKeySerializer());
        await serializer.DeserializeAsync(index, stream, false );

        CurrentIndex = index;

    }

    public async Task<IEnumerable<ResultDocument>> Search(string query)
    {


        using var logger = EnterMethod();

        logger.Inspect(nameof(query), query);



        // *****************************************************************
        logger.Debug("Attempting to check for new index");
        var found = await Load();
        if (!found && CurrentIndex is null)
            await BuildIndex();



        // *****************************************************************
        logger.Debug("Attempting to search");
        logger.Inspect(nameof(CurrentIndex), CurrentIndex is not null );

        ISearchResults<InputKey>? results;
        try
        {

            _lock.EnterReadLock();

            results = CurrentIndex?.Search(query);

        }
        finally
        {
            _lock.ExitReadLock();
        }



        // *****************************************************************
        logger.Debug("Attempting to sort by score descending and transform");

        static int Calc(double total, double field)
        {
            var p = (field / total) * 100;
            var res = Convert.ToInt32(Math.Round(p, MidpointRounding.AwayFromZero));
            return res;
        }

        var docs = new List<ResultDocument>();
        if( results is not null )
            docs = results.OrderByDescending(r => r.Score).Select(r =>
            {
                var total = r.Score;
                var explanation = string.Join( ", ", r.FieldMatches.Select( m => $"{m.FoundIn}: {Calc(total, m.Score)}%" ) );
                return ResultDocument.Build( r.Key, explanation, r.Score );
            }).ToList();



        // *****************************************************************
        return docs;


    }



    private class BuildRequest
    {
    }


    public void RequestRebuild()
    {

        Requests.Enqueue(new BuildRequest());

    }


    private ConcurrentQueue<BuildRequest> Requests { get; } = new();
    private ManualResetEvent MustStop { get; } = new(false);
    private ManualResetEvent Stopped { get; } = new(false);

    public Task Start()
    {

        using var logger = EnterMethod();

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

        await BuildIndex();

        loggerEn.Debug("Enter Run");
        loggerEn.Dispose();


        while (!MustStop.WaitOne(TimeSpan.FromMilliseconds(1000)))
        {

            if( Requests.TryDequeue(out _) )
            {

                try
                {

                    await BuildIndex();

                }
                catch (Exception cause)
                {
                    using var loggerEr = this.GetLogger();
                    loggerEr.Error(cause, "BuildIndex failed.");
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



}





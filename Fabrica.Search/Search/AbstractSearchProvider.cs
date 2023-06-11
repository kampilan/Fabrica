using Fabrica.Utilities.Container;
using Lifti;
using Lifti.Serialization.Binary;

namespace Fabrica.Search;

// ReSharper disable once UnusedTypeParameter
public abstract class AbstractSearchProvider<TDocument,TIndex>: CorrelatedObject, ISearchProvider<TIndex> where TDocument : class where TIndex: class
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


    protected AbstractSearchProvider( ICorrelation correlation, ISearchIndexState<TIndex> state ): base(correlation)
    {
        State    = state;
    }


    protected ISearchIndexState<TIndex> State { get; }


    protected abstract Task<IEnumerable<TDocument>> GetInputs();

    protected abstract FullTextIndex<InputKey> CreateIndexDefinition();

    public async Task BuildIndex()
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to create Index definition");
        var index = CreateIndexDefinition();



        // *****************************************************************
        logger.Debug("Attempting to get Input documents");
        var inputs = await GetInputs();

        await index.AddRangeAsync(inputs);


        using var ms = new MemoryStream();

        var serializer = new BinarySerializer<InputKey>(new InputKeySerializer());
        await serializer.SerializeAsync(index, ms, false);

        await State.SetState(ms.ToArray());


    }


    protected async Task<FullTextIndex<InputKey>> GetIndex()
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to create Index def");
        var index = CreateIndexDefinition();



        // *****************************************************************
        logger.Debug("Attempting to check state");
        await State.CheckState();



        // *****************************************************************
        logger.Debug("Attempting to read State");
        await State.ReadState(async m =>
        {
            using var ms = new MemoryStream(m.ToArray());
            var serializer = new BinarySerializer<InputKey>(new InputKeySerializer());
            await serializer.DeserializeAsync(index, ms);

        });



        // *****************************************************************
        return index;


    }


    public async Task<IEnumerable<ResultDocument>> Search(string query)
    {


        using var logger = EnterMethod();

        logger.Inspect(nameof(query), query);



        // *****************************************************************
        logger.Debug("Attempting to get Index");
        var index = await GetIndex();



        // *****************************************************************
        logger.Debug("Attempting to search");
        var results = index.Search(query);



        // *****************************************************************
        logger.Debug("Attempting to sort by score descending and transform");
        var docs = results.OrderByDescending(r => r.Score).Select(r => ResultDocument.Build(r.Key, r.Score)).ToList();



        // *****************************************************************
        return docs;


    }



}





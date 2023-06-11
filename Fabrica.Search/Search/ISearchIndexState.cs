namespace Fabrica.Search;

// ReSharper disable once UnusedTypeParameter
public interface ISearchIndexState<TIndex> where TIndex : class
{

    Task CheckState();

    Task ReadState(Func<Memory<byte>, Task> reader );

    Task SetState( Memory<byte> source );

}
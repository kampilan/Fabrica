namespace Fabrica.Search;

// ReSharper disable once UnusedTypeParameter
public interface ISearchProvider<TIndex> where TIndex: class
{

    void RequestRebuild();

    Task<IEnumerable<ResultDocument>> Search(string query);

}


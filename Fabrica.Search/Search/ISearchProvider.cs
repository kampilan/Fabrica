namespace Fabrica.Search;

// ReSharper disable once UnusedTypeParameter
public interface ISearchProvider<TIndex> where TIndex: class
{

    Task<IEnumerable<ResultDocument>> Search(string query);

    Task BuildIndex();

}
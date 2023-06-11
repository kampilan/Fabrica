namespace Fabrica.Search;

public interface ISearchIndexBuildRequestHandler<TIndex> where TIndex : class
{

    Task RequestBuild();

}
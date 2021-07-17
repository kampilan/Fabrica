namespace Fabrica.Press.Generation.DataSources
{
    public interface IMergeDataSource
    {
        string Region { get; }
        void Rewind();
        bool MoveNext();
        bool TryGetValue( string spec, out object value );
    }
}
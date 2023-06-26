using Fabrica.Watch.Sink;

namespace Fabrica.Watch;

public interface IWatchObjectSerializer
{

    ( PayloadType type, string payload) Serialize( object? source );

}
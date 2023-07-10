using Fabrica.Watch.Sink;

namespace Fabrica.Watch;

public interface IWatchExceptionSerializer
{

    (PayloadType type, string payload) Serialize( Exception error, object? context=null );

}
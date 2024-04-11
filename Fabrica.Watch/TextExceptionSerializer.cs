using Fabrica.Watch.Sink;
using Fabrica.Watch.Utilities;
using System.Text;
using System.Text.Json;

namespace Fabrica.Watch;

public class TextExceptionSerializer: IWatchExceptionSerializer
{


    public (PayloadType type, string payload) Serialize( Exception? error, object? context )
    {


        if (error is null)
            return (PayloadType.None, "");


        var builder = new StringBuilder();
        builder.AppendLine("");
        builder.AppendLine("");


        if( context is not null )
        {
            var json =  JsonSerializer.Serialize(context, JsonWatchObjectSerializer.WatchOptions);
            builder.AppendLine("--- Context -----------------------------------------");
            builder.AppendLine(json);
            builder.AppendLine();
        }


        builder.AppendLine("--- Exception ---------------------------------------");
        var inner = error;
        while (inner != null)
        {

            builder.AppendLine($" Exception: {inner.GetType().FullName} - {inner.Message}");

            builder.AppendLine();
            builder.AppendLine("--- Stack Trace --------------------------------------");
            builder.AppendLine(inner.StackTrace);
            builder.AppendLine("------------------------------------------------------");

            inner = inner.InnerException;

        }


        return (PayloadType.Text, builder.ToString());


    }


}
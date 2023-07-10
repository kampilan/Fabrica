using Fabrica.Watch.Sink;
using System.Text;
using Newtonsoft.Json;

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
            var json = JsonConvert.SerializeObject(context, Formatting.Indented);
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
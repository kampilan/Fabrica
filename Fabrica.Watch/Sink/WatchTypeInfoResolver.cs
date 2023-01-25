using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Fabrica.Watch.Sink;

public class WatchTypeInfoResolver: DefaultJsonTypeInfoResolver
{


    public override JsonTypeInfo GetTypeInfo( Type type, JsonSerializerOptions options )
    {

        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return typeInfo;

        foreach( var property in typeInfo.Properties )
        {

            if( property.PropertyType.GetCustomAttribute<SensitiveAttribute>() is not null )
                property.Get = o=> $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(o?.ToString())}";

        }

        return typeInfo;

    }


}
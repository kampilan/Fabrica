using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Fabrica.Watch.Sink;

namespace Fabrica.Watch.Utilities;

public class JsonWatchObjectSerializer: IWatchObjectSerializer
{

    static JsonWatchObjectSerializer()
    {

        WatchOptions = new(JsonSerializerDefaults.General)
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            TypeInfoResolver = new WatchJsonTypeInfoResolver()
        };

        WatchOptions.Converters.Add(new TypeWatchJsonConvert());

    }

    public static readonly JsonSerializerOptions WatchOptions;

    public (PayloadType type, string payload) Serialize(object? source)
    {

        var json = JsonSerializer.Serialize(source, WatchOptions);

        return (PayloadType.Json, json??"{}");

    }

}


internal class WatchJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{


    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {

        var typeInfo = base.GetTypeInfo(type, options);


        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return typeInfo;


        foreach (var prop in typeInfo.Properties)
        {
            
            var sensitive = prop.PropertyType.GetCustomAttribute<SensitiveAttribute>();
            if (sensitive is not null)
            {
                var og = prop.Get;
                prop.Get = o => SensitivePropertyGetter(o, og, prop.PropertyType);
            }
            else
            {
                var og = prop.Get;
                prop.Get = o => SafePropertyGetter(o, og, prop.PropertyType);
            }

        }

        return typeInfo;

    }

    private static object? SafePropertyGetter( object source, Func<object,object?>? getter, Type type )
    {

        object? Coerce()
        {
            var value = type.IsValueType ? Activator.CreateInstance(type) : null;
            return value;
        }

        try
        {

            if( getter is null)
                return Coerce();

            var value = getter(source);

            return value;

        }
        catch
        {
            return Coerce();
        }

    }


    private static object? SensitivePropertyGetter(object source, Func<object, object?>? getter, Type type)
    {

        object? Coerce()
        {
            var value = type.IsValueType ? Activator.CreateInstance(type) : null;
            return value;
        }

        try
        {

            if (getter is null)
                return Coerce();

            var value = getter(source);

            if( value is string s )
            {
                var sv = $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(s)}";
                return sv;
            }

            return value;

        }
        catch
        {
            return Coerce();
        }

    }


}

internal class TypeWatchJsonConvert: JsonConverter<Type>
{
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteStringValue(value.GetConciseFullName());
        writer.WriteEndObject();
    }
}
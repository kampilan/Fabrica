using System.Collections;
using Fabrica.Models.Support;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Models.Serialization;

public class ModelJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {


        var typeInfo = base.GetTypeInfo(type, options);

        if( typeInfo.Kind != JsonTypeInfoKind.Object )
            return typeInfo;

        var modelAttr = type.GetCustomAttribute<ModelAttribute>();
        if( modelAttr is null )
            return typeInfo;


        foreach (var prop in typeInfo.Properties)
        {

            ModelMetaAttribute? attr = null;
            if (prop.AttributeProvider is not null && prop.AttributeProvider.IsDefined(typeof(ModelMetaAttribute), false))
                attr = prop.AttributeProvider.GetCustomAttributes(typeof(ModelMetaAttribute), false).Cast<ModelMetaAttribute>().First();


            if (attr is null || attr.Scope == PropertyScope.Exclude)
            {
                prop.Get = null;
                prop.Set = null;
            }
            else if (attr.Scope == PropertyScope.Immutable)
            {
                prop.Set = null;
            }
            else if (prop.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
            {

                prop.ShouldSerialize = Should;

                static bool Should(object o, object? value)
                {
                    var result = true;
                    if (value is IEnumerable enumerable)
                    {
                        var e = enumerable.GetEnumerator();
                        using var unknown = e as IDisposable;
                        result = e.MoveNext();
                    }
                    return result;
                }

            }
            else
            {
                var field = $"_{char.ToLowerInvariant(prop.Name[0])}{prop.Name.Substring(1)}";
                var fi = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
                prop.Set = fi is not null ? fi.SetValue : null;
            }


        }


        return typeInfo;

    }

}
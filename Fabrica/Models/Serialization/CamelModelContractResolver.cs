using System.Collections;
using System.Reflection;
using Fabrica.Models.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Fabrica.Models.Serialization;

public class CamelModelContractResolver : CamelCasePropertyNamesContractResolver
{


    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {


        // ****************************************************************************
        var property = base.CreateProperty(member, memberSerialization);


        // ****************************************************************************
        if (!property.Writable && member is PropertyInfo pi && (typeof(IReferenceModel).IsAssignableFrom(pi.DeclaringType) || pi.Name == "Uid"))
            property.Writable = pi.SetMethod != null;


        // ****************************************************************************
        var mm = member.GetCustomAttribute<ModelMetaAttribute>();
        if (mm != null)
        {
            property.ShouldSerialize = _ => !mm.Ignore;
            return property;
        }


        // ****************************************************************************
        var nea = member.GetCustomAttribute<ExcludeEmptyAttribute>();
        if (nea != null && property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        {
            property.ShouldSerialize = instance =>
            {
                var prop = instance.GetType().GetProperty(member.Name);
                if (prop != null && prop.GetValue(instance, null) is IEnumerable enumerable)
                    return enumerable.GetEnumerator().MoveNext();
                else
                    return true;
            };

            return property;

        }



        // ****************************************************************************
        return property;


    }



}
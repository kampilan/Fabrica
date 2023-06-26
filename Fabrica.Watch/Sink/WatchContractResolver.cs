using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Fabrica.Watch.Sink;

public class WatchContractResolver : DefaultContractResolver
{

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {

        var mem = base.CreateProperty(member, memberSerialization);

        if( member is not PropertyInfo propInfo )
            return mem;

        if( propInfo.GetCustomAttribute<SensitiveAttribute>() != null )
            mem.ValueProvider = new SensitiveValueProvider(propInfo);

        return mem;

    }


}

public class SensitiveValueProvider : IValueProvider
{


    public SensitiveValueProvider(PropertyInfo propInfo)
    {
        PropInfo = propInfo;
    }


    private PropertyInfo PropInfo { get; }


    public void SetValue(object target, object? value)
    {
        PropInfo.SetValue(target, value);
    }

    public object GetValue(object target)
    {

        var value = PropInfo.GetValue(target);
        var strVal = value?.ToString();

        var sub = $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(strVal)}";

        return sub;

    }


}
using System.Reflection;

namespace Fabrica.Watch.Sink;

public class SensitiveValueProvider
{


    public SensitiveValueProvider(PropertyInfo propInfo)
    {
        PropInfo = propInfo;
    }


    private PropertyInfo PropInfo { get; }


    public void SetValue(object target, object value)
    {
        PropInfo.SetValue( target, value );
    }
        
    public object GetValue(object target)
    {

        var value  = PropInfo.GetValue(target);
        var strVal = value?.ToString();

        var sub = $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(strVal)}";

        return sub;

    }


}
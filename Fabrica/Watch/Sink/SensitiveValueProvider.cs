using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;

namespace Fabrica.Watch.Sink
{

    
    public class SensitiveValueProvider: IValueProvider
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

        [NotNull]
        public object GetValue(object target)
        {

            var value  = PropInfo.GetValue(target);
            var strVal = value?.ToString();

            var sub = $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(strVal)}";

            return sub;

        }


    }


}

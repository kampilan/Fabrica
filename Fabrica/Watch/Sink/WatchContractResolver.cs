using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Fabrica.Watch.Sink
{


    public class WatchContractResolver: DefaultContractResolver
    {


        protected override JsonProperty CreateProperty( MemberInfo member, MemberSerialization memberSerialization )
        {
            
            var mem = base.CreateProperty(member, memberSerialization);

            if( !(member is PropertyInfo propInfo) ) 
                return mem;

            if( propInfo.GetCustomAttribute<SensitiveAttribute>() != null )
                mem.ValueProvider = new SensitiveValueProvider(propInfo);

            return mem;

        }


    }


}

using System.Collections.Generic;

namespace Fabrica.Mediator.Requests
{
 
    public abstract class BaseUpdateRequest: IUpdateRequest
    {

        public string Uid { get; set; } = "";
    
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    }


}

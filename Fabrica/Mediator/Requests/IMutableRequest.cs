using System.Collections.Generic;

namespace Fabrica.Mediator.Requests
{

    public interface IMutableRequest
    {

        IDictionary<string, object> Properties { get; set; }

    }


}

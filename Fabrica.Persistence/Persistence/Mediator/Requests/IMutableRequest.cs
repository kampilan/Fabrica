using System.Collections.Generic;

namespace Fabrica.Persistence.Mediator.Requests
{

    public interface IMutableRequest
    {

        IDictionary<string, object> Properties { get; }

    }


}

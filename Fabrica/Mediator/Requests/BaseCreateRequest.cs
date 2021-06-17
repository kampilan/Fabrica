using System.Collections.Generic;

namespace Fabrica.Mediator.Requests
{

    public abstract class BaseCreateRequest: ICreateRequest
    {

        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    }

}

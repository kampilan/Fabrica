using System.Collections.Generic;

namespace Fabrica.Mediator.Requests
{

    public abstract class BaseMemberCreateRequest: IMemberCreateRequest
    {

        public string ParentUid { get; set; } = "";

        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    }

}

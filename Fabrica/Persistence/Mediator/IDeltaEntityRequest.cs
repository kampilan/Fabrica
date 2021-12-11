using System.Collections.Generic;

namespace Fabrica.Persistence.Mediator;

public interface IDeltaEntityRequest
{

    public string Uid { get; set; }

    public Dictionary<string,object> Delta { get; set; }


}
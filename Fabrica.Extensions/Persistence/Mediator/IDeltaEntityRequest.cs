using System.Collections.Generic;

namespace Fabrica.Persistence.Mediator;

public interface IDeltaEntityRequest
{

    public OperationType Operation { get; set; }

    public string Uid { get; }

    public IDictionary<string,object> Delta { get; }


}
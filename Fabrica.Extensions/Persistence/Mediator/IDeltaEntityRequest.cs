using Fabrica.Models.Support;

namespace Fabrica.Persistence.Mediator;

public interface IDeltaEntityRequest
{

    public OperationType Operation { get; set; }

    public string Uid { get; }

    public BaseDelta Delta { get; }


}
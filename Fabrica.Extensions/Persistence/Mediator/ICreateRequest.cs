using Fabrica.Models.Support;

namespace Fabrica.Persistence.Mediator;

public interface ICreateRequest
{

    public string Uid { get; }

    public BaseDelta Delta { get; }


}
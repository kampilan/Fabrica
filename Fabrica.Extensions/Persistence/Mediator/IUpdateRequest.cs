
using Fabrica.Models.Support;

namespace Fabrica.Persistence.Mediator;

public interface IUpdateRequest
{

    public string Uid { get; }

    public BaseDelta Delta { get; }


}
using Fabrica.Models.Support;

namespace Fabrica.Mediator.Requests
{

    public interface IDeltaRequest
    {

        string Uid { get; set; }

        BaseDelta Delta { get; }

    }


}

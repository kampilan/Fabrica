using Fabrica.Models.Support;

namespace Fabrica.Mediator.Requests
{

    public interface IMutableRequest
    {

        BaseDelta Delta { get; }

    }


}

using Fabrica.Models.Support;

namespace Fabrica.Mediator.Requests
{


    public interface IUpdateRequest: IMutableRequest
    {

        string Uid { get; set; }

    }


}

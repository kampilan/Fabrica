using Fabrica.Persistence.Mediator;

namespace Fabrica.Mediator.Requests
{

    public abstract class BaseRetrieveRequest: IRetrieveRequest
    {

        public string Uid { get; set; } = "";

    }


}

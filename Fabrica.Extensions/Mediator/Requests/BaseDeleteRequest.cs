using Fabrica.Persistence.Mediator;

namespace Fabrica.Mediator.Requests
{

    public abstract class BaseDeleteRequest: IDeleteRequest
    {
    
        public string Uid { get; set; } = "";


    }


}
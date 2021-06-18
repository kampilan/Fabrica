using Fabrica.Models.Support;

namespace Fabrica.Mediator.Requests
{


    public abstract class BaseCreateRequest<TDelta>: ICreateRequest where TDelta: BaseDelta, new()
    {

        public TDelta Delta { get; set; } = new ();

        BaseDelta IMutableRequest.Delta => Delta;

    }

}

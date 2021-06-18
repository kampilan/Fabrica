using Fabrica.Models.Support;

namespace Fabrica.Mediator.Requests
{


    public abstract class BaseUpdateRequest<TDelta> : IUpdateRequest where TDelta: BaseDelta, new()
    {

        public string Uid { get; set; } = "";

        public TDelta Delta { get; set; } = new ();

        BaseDelta IMutableRequest.Delta => Delta;

    }


}

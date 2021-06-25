using Fabrica.Models.Support;

namespace Fabrica.Mediator.Requests
{


    public abstract class BaseDeltaRequest<TDelta> : IDeltaRequest where TDelta: BaseDelta, new()
    {

        public string Uid { get; set; } = "";

        public TDelta Delta { get; set; } = new ();

        BaseDelta IDeltaRequest.Delta => Delta;

    }


}

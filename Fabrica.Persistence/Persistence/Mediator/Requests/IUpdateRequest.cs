namespace Fabrica.Persistence.Mediator.Requests
{


    public interface IUpdateRequest: IMutableRequest
    {
        string Uid { get; }

    }


}

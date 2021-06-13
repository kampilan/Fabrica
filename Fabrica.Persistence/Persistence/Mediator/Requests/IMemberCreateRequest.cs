namespace Fabrica.Persistence.Mediator.Requests
{

    public interface IMemberCreateRequest: IMutableRequest
    {

        string ParentUid { get; }

    }

}

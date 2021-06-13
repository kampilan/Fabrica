namespace Fabrica.Mediator.Requests
{

    public interface IMemberCreateRequest: IMutableRequest
    {

        string ParentUid { get; set; }

    }

}

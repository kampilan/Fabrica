// ReSharper disable UnusedMember.Global

namespace Fabrica.Mediator;

public class BatchResponse
{

    private List<IResponse> Responses { get; } = new();

    public void Add( IResponse response ) => Responses.Add(response);

    public void EnsureSuccess()
    {

        var error = Responses.FirstOrDefault(r => !r.Ok);
        if( error is not null )
            throw new MediatorException(error);

    }


    public bool HasErrors => Responses.Any(r => !r.Ok);
    public IEnumerable<IResponse> Errors => Responses.Where(r => !r.Ok);


}

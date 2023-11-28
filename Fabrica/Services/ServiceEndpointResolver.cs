namespace Fabrica.Services;

public class ServiceEndpointResolver
{

    public ServiceEndpointResolver(IEnumerable<ServiceEndpoint> endpoints)
    {

        foreach (var ep in endpoints)
            Endpoints[ep.FullyQualifiedName] = ep;

    }
    private Dictionary<string, ServiceEndpoint> Endpoints { get; } = new();


    public ServiceEndpoint? GetEndpoint(string name)
    {
        if (Endpoints.TryGetValue(name, out var ep))
            return ep;
        return null;
    }

}
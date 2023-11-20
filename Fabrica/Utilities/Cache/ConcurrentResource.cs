// ReSharper disable UnusedMember.Global
namespace Fabrica.Utilities.Cache;

public class ConcurrentResource<T> : AbstractConcurrentResource<T>
{

    public ConcurrentResource(Func<Task<IRenewedResource<T>>> factory)
    {
        Factory = factory;
    }

    private Func<Task<IRenewedResource<T>>> Factory { get; }

    protected override async Task<IRenewedResource<T>> Renew()
    {
        var r = await Factory();
        return r;
    }

    public bool HasExpired => Expiration < DateTime.Now;


}
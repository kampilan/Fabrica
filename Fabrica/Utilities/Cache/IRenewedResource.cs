namespace Fabrica.Utilities.Cache;

public interface IRenewedResource<T>
{
    T Value { get; }

    TimeSpan TimeToRenew { get; }
    TimeSpan TimeToLive { get; }
}
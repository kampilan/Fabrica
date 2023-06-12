namespace Fabrica.Utilities.Cache;

public class RenewedResource<T> : IRenewedResource<T>
{

    public required T Value { get; init; } = default!;
    public TimeSpan TimeToRenew { get; set; } = TimeSpan.MaxValue;
    public TimeSpan TimeToLive { get; set; } = TimeSpan.MaxValue;

}
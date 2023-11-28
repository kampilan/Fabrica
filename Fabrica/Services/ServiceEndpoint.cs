using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Services;

public class ServiceEndpoint
{

    public required string ServiceName { get; init; }
    public required string EndpointName { get; init; }

    public required Uri BaseAddress { get; init; }

    public HttpMethod Method { get; init; } = HttpMethod.Post;
    public required Uri Path { get; init; }

    public AuthenticationType Authentication { get; init; } = AuthenticationType.None;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);
    public int RetryCount { get; init; } = 3;

    public virtual IAsyncPolicy<HttpResponseMessage> GetPolicy()
    {

        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), RetryCount, fastFirst: true);
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(Timeout);

        var policy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

        return policy;

    }


    public string FullyQualifiedName => $"{ServiceName}:{EndpointName}";

    private Uri? _endpoint;
    public Uri Endpoint => _endpoint ??= new Uri(BaseAddress, Path);

}
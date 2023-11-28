using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.Extensions.Http;
using Polly;

namespace Fabrica.Services;

public class ServiceMessageHandler : DelegatingHandler
{

    public ServiceMessageHandler( ICorrelation correlation, ServiceEndpoint ep ): this(correlation, ep.Method, ep.Authentication, ep.GetPolicy())
    {
    }

    public ServiceMessageHandler(ICorrelation correlation, HttpMethod method, AuthenticationType authType, IAsyncPolicy<HttpResponseMessage>? policy=null)
    {

        Correlation = correlation;
        Method      = method;
        AuthType    = authType;

        if (policy is not null)
        {
            InnerHandler = new PolicyHttpMessageHandler(policy)
            {
                InnerHandler = new HttpClientHandler()
            };
        }
        else
        {
            InnerHandler = new HttpClientHandler();
        }


    }

    protected ICorrelation Correlation { get; }
    protected HttpMethod Method { get;  }
    protected AuthenticationType AuthType { get; }

    protected ILogger EnterMethod([CallerMemberName] string name = "")
    {
        return Correlation.EnterMethod(GetType(), name);
    }

    protected virtual void Configure( HttpRequestMessage request )
    {

        request.Method = Method;

        switch (AuthType)
        {
            case AuthenticationType.None:
                break;
            case AuthenticationType.Gateway:
                if( !string.IsNullOrWhiteSpace( Correlation.CallerGatewayToken) )
                    request.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", Correlation.CallerGatewayToken );
                break;
            default:
                throw new NotImplementedException($"{AuthType} Authentication type not yet supported");

        }


    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to configure HttpRequest");
        Configure(request);

        if( logger.IsDebugEnabled )
        {
            var obj = new { Uri = request.RequestUri?.ToString() ?? "", Method = request.Method.ToString(), ContentType = request.Content?.Headers.ContentType?.ToString() ?? "", ContentLength = request.Content?.Headers.ContentLength ?? 0 };
            logger.LogObject("Request", obj);
        }



        // *****************************************************************
        logger.Debug("Attempting to send HttpRequest");
        var response = await base.SendAsync(request, cancellationToken);

        if( logger.IsDebugEnabled )
        {
            var obj = new { response.StatusCode, response.IsSuccessStatusCode, ContentType = response.Content?.Headers.ContentType?.ToString() ?? "", ContentLength = response.Content?.Headers.ContentLength ?? -1 };
            logger.LogObject("Response", obj);
        }


        // *****************************************************************
        return response;

    }

}
using Fabrica.Http;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.Persistence.Http.Mediator.Handlers;

public abstract class BaseHttpHandler<TRequest, TResponse> : AbstractRequestHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>
{


    protected BaseHttpHandler(ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta ) : base(correlation)
    {

        Factory = factory;
        Meta    = meta;

    }


    protected IHttpClientFactory Factory { get; }
    protected IModelMetaService Meta { get; }


    protected virtual async Task<HttpResponse> Send(HttpRequest request, CancellationToken token)
    {


        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to create Api HttpClient");
        using var client = Factory.CreateClient(request.HttpClientName);

        if (client.BaseAddress is null)
            throw new InvalidOperationException($"HttpClient: ({request.HttpClientName}) has a null BaseAddress");



        logger.Inspect(nameof(client.BaseAddress), client.BaseAddress);
        logger.Inspect(nameof(request.Method), request.Method);
        logger.Inspect(nameof(request.Path), request.Path);
        logger.Inspect(nameof(request.BodyContent), request.BodyContent is not null);





        // *****************************************************************
        logger.Debug("Attempting to build Inner Request");
        var innerRequest = new HttpRequestMessage
        {
            Method     = request.Method,
            RequestUri = new Uri( client.BaseAddress, request.Path )
        };


        // *****************************************************************
        logger.Debug("Attempting to add custom headers");
        foreach (var (key, value) in request.CustomHeaders)
        {
            logger.DebugFormat("{0} = ({1})", key, value);
            innerRequest.Headers.Add(key, value);
        }



        // *****************************************************************
        logger.Debug("Attempting to add body content");
        if (request.BodyContent is not null)
            innerRequest.Content = request.BodyContent;



        // *****************************************************************
        logger.Debug("Attempting to Send request");
        var innerResponse = await client.SendAsync(innerRequest, token);

        logger.Inspect(nameof(innerResponse.StatusCode), innerResponse.StatusCode);

        innerResponse.EnsureSuccessStatusCode();



        // *****************************************************************
        logger.Debug("Attempting to read body content");
        var content = await innerResponse.Content.ReadAsStringAsync(token);



        // *****************************************************************
        logger.Debug("Attempting to build response");
        var response = new HttpResponse(innerResponse.StatusCode, "", true, content);


        // *****************************************************************
        return response;


    }


}



public abstract class BaseHttpHandler<TRequest> : AbstractRequestHandler<TRequest> where TRequest : class, IRequest<Response>
{


    protected BaseHttpHandler(ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) : base(correlation)
    {

        Factory = factory;
        Meta = meta;

    }


    protected IHttpClientFactory Factory { get; }
    protected IModelMetaService Meta { get; }


    protected virtual async Task<HttpResponse> Send(HttpRequest request, CancellationToken token)
    {


        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to create Api HttpClient");
        using var client = Factory.CreateClient(request.HttpClientName);

        if (client.BaseAddress is null)
            throw new InvalidOperationException($"HttpClient: ({request.HttpClientName}) has a null BaseAddress");



        logger.Inspect(nameof(client.BaseAddress), client.BaseAddress);
        logger.Inspect(nameof(request.Method), request.Method);
        logger.Inspect(nameof(request.Path), request.Path);
        logger.Inspect(nameof(request.BodyContent), request.BodyContent is not null);





        // *****************************************************************
        logger.Debug("Attempting to build Inner Request");
        var innerRequest = new HttpRequestMessage
        {
            Method = request.Method,
            RequestUri = new Uri(client.BaseAddress, request.Path)
        };


        // *****************************************************************
        logger.Debug("Attempting to add custom headers");
        foreach (var (key, value) in request.CustomHeaders)
        {
            logger.DebugFormat("{0} = ({1})", key, value);
            innerRequest.Headers.Add(key, value);
        }



        // *****************************************************************
        logger.Debug("Attempting to add body content");
        if (request.BodyContent is not null)
            innerRequest.Content = request.BodyContent;



        // *****************************************************************
        logger.Debug("Attempting to Send request");
        var innerResponse = await client.SendAsync(innerRequest, token);

        logger.Inspect(nameof(innerResponse.StatusCode), innerResponse.StatusCode);

        innerResponse.EnsureSuccessStatusCode();



        // *****************************************************************
        logger.Debug("Attempting to read body content");
        var content = await innerResponse.Content.ReadAsStringAsync(token);



        // *****************************************************************
        logger.Debug("Attempting to build response");
        var response = new HttpResponse(innerResponse.StatusCode, "", true, content);


        // *****************************************************************
        return response;


    }


}
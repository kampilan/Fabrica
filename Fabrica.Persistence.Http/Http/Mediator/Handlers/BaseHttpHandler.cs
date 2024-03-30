using Fabrica.Http;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;

namespace Fabrica.Persistence.Http.Mediator.Handlers;

public abstract class BaseHttpHandler<TRequest, TResponse> : AbstractRequestHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>
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

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to create Api HttpClient");
            using var client = Factory.CreateClient(request.HttpClientName);

            if (client.BaseAddress is null)
                throw new InvalidOperationException($"HttpClient: ({request.HttpClientName}) has a null BaseAddress");

            

            logger.Inspect(nameof(client.BaseAddress), client.BaseAddress);
            logger.Inspect(nameof(request.Method), request.Method);
            logger.Inspect(nameof(request.Path), request.Path);
            logger.Inspect(nameof(request.BodyContent), request.BodyContent != null);



            // *****************************************************************
            logger.Debug("Attempting to build Inner Request");
            var innerRequest = new HttpRequestMessage
            {
                Method = request.Method,
                RequestUri = new Uri(client.BaseAddress, request.Path)
            };



            // *****************************************************************
            logger.Debug("Attempting to add custom headers");
            foreach (var pair in request.CustomHeaders)
            {
                logger.Debug("{0} = ({1})", pair.Key, pair.Value);
                innerRequest.Headers.Remove(pair.Key);
                innerRequest.Headers.Add(pair.Key, pair.Value);
            }



            // *****************************************************************
            logger.Debug("Attempting to add body content");
            if (request.BodyContent != null)
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
        finally
        {
            logger.LeaveMethod();
        }


    }


}



public abstract class BaseHttpHandler<TRequest> : AbstractRequestHandler<TRequest> where TRequest : class, IRequest<Response>
{


    protected BaseHttpHandler(ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) :
        base(correlation)
    {

        Factory = factory;
        Meta = meta;

    }


    protected IHttpClientFactory Factory { get; }
    protected IModelMetaService Meta { get; }


    protected virtual async Task<HttpResponse> Send(HttpRequest request, CancellationToken token)
    {

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to create Api HttpClient");
            using var client = Factory.CreateClient(request.HttpClientName);

            if (client.BaseAddress is null)
                throw new InvalidOperationException($"HttpClient: ({request.HttpClientName}) has a null BaseAddress");



            logger.Inspect(nameof(client.BaseAddress), client.BaseAddress);
            logger.Inspect(nameof(request.Method), request.Method);
            logger.Inspect(nameof(request.Path), request.Path);
            logger.Inspect(nameof(request.BodyContent), request.BodyContent != null);





            // *****************************************************************
            logger.Debug("Attempting to build Inner Request");
            var innerRequest = new HttpRequestMessage
            {
                Method = request.Method,
                RequestUri = new Uri(client.BaseAddress, request.Path)
            };


            // *****************************************************************
            logger.Debug("Attempting to add custom headers");
            foreach (var pair in request.CustomHeaders)
            {
                logger.Debug("{0} = ({1})", pair.Key, pair.Value);
                innerRequest.Headers.Add(pair.Key, pair.Value);
            }



            // *****************************************************************
            logger.Debug("Attempting to add body content");
            if (request.BodyContent != null)
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
        finally
        {
            logger.LeaveMethod();
        }


    }


}
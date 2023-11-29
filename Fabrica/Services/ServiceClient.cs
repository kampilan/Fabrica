using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.Extensions.Http;

namespace Fabrica.Services;

public class ServiceClient : CorrelatedObject
{

    private static readonly JsonSerializerOptions Options = new();

    public ServiceClient(ICorrelation correlation, ServiceEndpointResolver resolver) : base(correlation)
    {

        Resolver = resolver;

    }
    private ServiceEndpointResolver Resolver { get; }

    private HttpClient CreateClient(string endpointName)
    {

        var ep = Resolver.GetEndpoint(endpointName);
        if( ep is null )
            throw new InvalidOperationException($"Endpoint: ({endpointName}) was not found");

        return CreateClient(ep);

    }

    private HttpClient CreateClient( ServiceEndpoint ep )
    {

        var handler = new ServiceMessageHandler(Correlation, ep);

        var client = new HttpClient(handler, true)
        {
            BaseAddress = ep.Endpoint
        };

        return client;

    }



    public async Task<TResponse> Request<TResponse>(string endpointName, object? body=null ) where TResponse : class
    {

        using var logger = EnterMethod();


        if (logger.IsDebugEnabled)
        {
            var obj = new { EndpointName = endpointName, BodyType = body?.GetType().FullName??"", ResponseType = typeof(TResponse).FullName };
            logger.LogObject("Request", obj);
        }


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            using var client = CreateClient(endpointName);



            // *****************************************************************
            var httpReq = new HttpRequestMessage();
            if ( body is not null )
            {
                logger.Debug("Attempting to serialize body into request content");
                var content = JsonContent.Create(body, options: Options);
                httpReq.Content = content;
            }



            // *****************************************************************
            logger.Debug("Attempting to send via HttpClient");
            var httpRes = await client.SendAsync(httpReq);

            logger.LogObject(nameof(httpRes), httpRes);



            // *****************************************************************
            logger.Debug("Attempting to ensure success");
            httpRes.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to parse result into Response");
            var response = await httpRes.Content.ReadFromJsonAsync<TResponse>();
            if (response is null)
                throw new InvalidOperationException($"Endpoint: {endpointName} - Failed to parse return JSON into response");



            // *****************************************************************
            return response;


        }
        catch (Exception cause)
        {
            var obj = new { EndpointName = endpointName, BodyType = body?.GetType().FullName??"", ResponseType = typeof(TResponse).FullName };
            logger.ErrorWithContext(cause, obj, "Service client Send Failed.");
            throw;
        }


    }


    public async Task<TResponse> Request<TResponse>(ServiceEndpoint ep, object? body = null) where TResponse : class
    {

        using var logger = EnterMethod();


        if (logger.IsDebugEnabled)
        {
            var obj = new { Endpoint = ep, BodyType = body?.GetType().FullName ?? "", ResponseType = typeof(TResponse).FullName };
            logger.LogObject("Request", obj);
        }


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            using var client = CreateClient(ep);



            // *****************************************************************
            var httpReq = new HttpRequestMessage();
            if (body is not null)
            {
                logger.Debug("Attempting to serialize body into request content");
                var content = JsonContent.Create(body, options: Options);
                httpReq.Content = content;
            }



            // *****************************************************************
            logger.Debug("Attempting to send via HttpClient");
            var httpRes = await client.SendAsync(httpReq);

            logger.LogObject(nameof(httpRes), httpRes);



            // *****************************************************************
            logger.Debug("Attempting to ensure success");
            httpRes.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to parse result into Response");
            var response = await httpRes.Content.ReadFromJsonAsync<TResponse>();
            if (response is null)
                throw new InvalidOperationException($"Endpoint: {ep.FullyQualifiedName} - Failed to parse return JSON into response");



            // *****************************************************************
            return response;


        }
        catch (Exception cause)
        {
            var obj = new { Endpoint = ep, BodyType = body?.GetType().FullName ?? "", ResponseType = typeof(TResponse).FullName };
            logger.ErrorWithContext(cause, obj, "Service client Send Failed.");
            throw;
        }


    }


    public async Task<(MemoryStream content,string contentType)> RequestAsStream( ServiceEndpoint ep, string? body=""  )
    {

        using var logger = EnterMethod();


        if (logger.IsDebugEnabled)
        {
            var obj = new { Endpoint = ep };
            logger.LogObject("Request", obj);
        }


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            using var client = CreateClient(ep);



            // *****************************************************************
            var httpReq = new HttpRequestMessage();
            if (!string.IsNullOrWhiteSpace(body))
            {
                logger.Debug("Attempting to put body into request content");
                var content = new StringContent( body, Encoding.UTF8,  "application/json" );
                httpReq.Content = content;
            }



            // *****************************************************************
            logger.Debug("Attempting to send via HttpClient");
            var httpRes = await client.SendAsync(httpReq);

            logger.LogObject(nameof(httpRes), httpRes);



            // *****************************************************************
            logger.Debug("Attempting to ensure success");
            httpRes.EnsureSuccessStatusCode();


            // *****************************************************************
            logger.Debug("Attempting to dig out Content-Type and content");
            var contentType = httpRes.Content.Headers.ContentType?.MediaType??"";

            var strm = new MemoryStream();
            await httpRes.Content.CopyToAsync(strm);

            strm.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            return (strm,contentType);


        }
        catch (Exception cause)
        {
            var obj = new { Endpoint = ep };
            logger.ErrorWithContext(cause, obj, "Service client Send Failed.");
            throw;
        }


    }


    public async Task Execute(string endpointName, object? body=null)
    {

        using var logger = EnterMethod();


        if (logger.IsDebugEnabled)
        {
            var obj = new { EndpointName = endpointName, BodyType = body?.GetType().FullName??"" };
            logger.LogObject("Request", obj);
        }


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            using var client = CreateClient(endpointName);



            // *****************************************************************
            logger.Debug("Attempting to serialize body into request content");
            var httpReq = new HttpRequestMessage();
            if (body is not null)
            {
                logger.Debug("Attempting to serialize body into request content");
                var content = JsonContent.Create(body, options: Options);
                httpReq.Content = content;
            }




            // *****************************************************************
            logger.Debug("Attempting to send via HttpClient");
            var httpRes = await client.SendAsync(httpReq);

            logger.LogObject(nameof(httpRes), httpRes);



            // *****************************************************************
            logger.Debug("Attempting to ensure success");
            httpRes.EnsureSuccessStatusCode();


        }
        catch (Exception cause)
        {
            var obj = new { EndpointName = endpointName, BodyType = body?.GetType().FullName??"" };
            logger.ErrorWithContext(cause, obj, "Service client Send Failed.");
            throw;
        }

    }


    public async Task Execute( ServiceEndpoint ep, object? body = null )
    {

        using var logger = EnterMethod();


        if (logger.IsDebugEnabled)
        {
            var obj = new { Endpoint = ep, BodyType = body?.GetType().FullName ?? "" };
            logger.LogObject("Request", obj);
        }


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            using var client = CreateClient(ep);



            // *****************************************************************
            logger.Debug("Attempting to serialize body into request content");
            var httpReq = new HttpRequestMessage();
            if (body is not null)
            {
                logger.Debug("Attempting to serialize body into request content");
                var content = JsonContent.Create(body, options: Options);
                httpReq.Content = content;
            }




            // *****************************************************************
            logger.Debug("Attempting to send via HttpClient");
            var httpRes = await client.SendAsync(httpReq);

            logger.LogObject(nameof(httpRes), httpRes);



            // *****************************************************************
            logger.Debug("Attempting to ensure success");
            httpRes.EnsureSuccessStatusCode();


        }
        catch (Exception cause)
        {
            var obj = new { Endpoint = ep, BodyType = body?.GetType().FullName ?? "" };
            logger.ErrorWithContext(cause, obj, "Service client Send Failed.");
            throw;
        }

    }






}
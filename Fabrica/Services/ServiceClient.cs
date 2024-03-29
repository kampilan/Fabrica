﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Services;

public class ServiceClient : CorrelatedObject
{

    private static readonly JsonSerializerOptions Options = new();


    public static readonly string HttpClientName = "Fabrica.ServiceClient";


    public ServiceClient(ICorrelation correlation, ServiceEndpointResolver resolver, IHttpClientFactory factory ) : base(correlation)
    {

        _resolver = resolver;
        _factory  = factory;


    }

    private readonly ServiceEndpointResolver _resolver;
    private readonly IHttpClientFactory _factory;


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
            logger.Debug("Attempting to get ServiceEndpoint for given endpoint name");
            var ep = _resolver.GetEndpoint(endpointName);
            if( ep is null )
                throw new InvalidOperationException($"Endpoint: ({endpointName}) was not found");



            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            var client = _factory.CreateClient(HttpClientName);



            // *****************************************************************
            logger.Debug("Attempting to build HttpRequestMessage");
            var httpReq = new HttpRequestMessage
            {
                RequestUri = ep.Endpoint,
                Method     = ep.Method
            };

            if ( ep.Authentication == AuthenticationType.Gateway && !string.IsNullOrWhiteSpace(Correlation.CallerGatewayToken) )
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("bearer", Correlation.CallerGatewayToken);



            // *****************************************************************
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
            var client = _factory.CreateClient(HttpClientName);



            // *****************************************************************
            logger.Debug("Attempting to build HttpRequestMessage");
            var httpReq = new HttpRequestMessage
            {
                RequestUri = ep.Endpoint,
                Method = ep.Method
            };

            if (ep.Authentication == AuthenticationType.Gateway && !string.IsNullOrWhiteSpace(Correlation.CallerGatewayToken))
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("bearer", Correlation.CallerGatewayToken);



            // *****************************************************************
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


    public async Task<ContentStream> RequestAsStream( ServiceEndpoint ep, ContentStream? body=null  )
    {

        using var logger = EnterMethod();


        if( logger.IsDebugEnabled )
        {
            var obj = new { Endpoint = ep };
            logger.LogObject("Request", obj);
        }


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            var client = _factory.CreateClient(HttpClientName);



            // *****************************************************************
            logger.Debug("Attempting to build HttpRequestMessage");
            var httpReq = new HttpRequestMessage
            {
                RequestUri = ep.Endpoint,
                Method = ep.Method
            };

            if (ep.Authentication == AuthenticationType.Gateway && !string.IsNullOrWhiteSpace(Correlation.CallerGatewayToken))
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("bearer", Correlation.CallerGatewayToken);


            if ( body is not null )
            {

                logger.Debug("Attempting to put body into request content");
                var content = new StreamContent(body);
                content.Headers.ContentType = body.ContentType;

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
            var contentType = httpRes.Content.Headers.ContentType??new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

            var strm = new ContentStream();
            strm.ContentType = contentType;

            await httpRes.Content.CopyToAsync(strm);

            strm.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            return strm;


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
            logger.Debug("Attempting to get ServiceEndpoint for given endpoint name");
            var ep = _resolver.GetEndpoint(endpointName);
            if (ep is null)
                throw new InvalidOperationException($"Endpoint: ({endpointName}) was not found");



            // *****************************************************************
            logger.Debug("Attempting to get service Http client");
            var client = _factory.CreateClient(HttpClientName);



            // *****************************************************************
            logger.Debug("Attempting to build HttpRequestMessage");
            var httpReq = new HttpRequestMessage
            {
                RequestUri = ep.Endpoint,
                Method = ep.Method
            };

            if (ep.Authentication == AuthenticationType.Gateway && !string.IsNullOrWhiteSpace(Correlation.CallerGatewayToken))
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("bearer", Correlation.CallerGatewayToken);


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
            var client = _factory.CreateClient(HttpClientName);



            // *****************************************************************
            logger.Debug("Attempting to build HttpRequestMessage");
            var httpReq = new HttpRequestMessage
            {
                RequestUri = ep.Endpoint,
                Method = ep.Method
            };

            if (ep.Authentication == AuthenticationType.Gateway && !string.IsNullOrWhiteSpace(Correlation.CallerGatewayToken))
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("bearer", Correlation.CallerGatewayToken);


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
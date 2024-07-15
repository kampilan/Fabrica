using System.Text.Json;
using Fabrica.Watch;

namespace Fabrica.Http;

public static class HttpClientExtensions
{


    public static async Task<(bool ok, TResponse? model)> One<TResponse>(this IHttpClientFactory factory, HttpRequest request, CancellationToken ct = new()) where TResponse : class
    {

        using var client = factory.CreateClient(request.HttpClientName);

        var res = await client.One<TResponse>(request, ct);

        return res;

    }



    public static async Task<(bool ok, TResponse? model)> One<TResponse>(this HttpClient client, HttpRequest request, CancellationToken ct=new()) where TResponse : class
    {

        var logger = client.GetLogger();


        var res = await client.Send(request,ct, false);

        if (!res.WasSuccessful)
        {
            logger.Inspect(nameof(res.StatusCode), res.StatusCode);
            return (false, null);
        }



        // *****************************************************************
        logger.Debug("Attempting to read json");
        var json = res.Content;

        logger.LogJson(nameof(json), json);



        // *****************************************************************
        logger.Debug("Attempting to deserialize to model");
        var one = JsonSerializer.Deserialize<TResponse>(json);

        logger.LogObject(nameof(one), one);



        // *****************************************************************
        return (true, one);


    }


    public static async Task<(bool ok, IEnumerable<TResponse> results )> Many<TResponse>(this IHttpClientFactory factory, HttpRequest request, CancellationToken ct = new()) where TResponse : class
    {

        using var client = factory.CreateClient(request.HttpClientName);

        var res = await client.Many<TResponse>(request, ct);

        return res;

    }


    public static async Task<(bool ok, IEnumerable<TResponse> results )> Many<TResponse>(this HttpClient client, HttpRequest request, CancellationToken ct = new()) where TResponse : class
    {

        var logger = client.GetLogger();


        var res = await client.Send(request, ct, false);

        if (!res.WasSuccessful)
        {
            logger.Inspect(nameof(res.StatusCode), res.StatusCode);
            return (false, Enumerable.Empty<TResponse>());
        }



        // *****************************************************************
        logger.Debug("Attempting to read json");
        var json = res.Content;

        logger.LogJson(nameof(json), json);



        // *****************************************************************
        logger.Debug("Attempting to deserialize to model");
        var results = JsonSerializer.Deserialize<IEnumerable<TResponse>>(json) ?? Enumerable.Empty<TResponse>();



        // *****************************************************************
        return (true, results);


    }


    public static async Task<HttpResponse> Send(this IHttpClientFactory factory, HttpRequest request, CancellationToken ct = new(), bool throwException = true)
    {

        using var client = factory.CreateClient(request.HttpClientName);

        var res = await client.Send(request, ct, throwException);

        return res;

    }



    public static async Task<HttpResponse> Send( this HttpClient client, HttpRequest request, CancellationToken ct=new(), bool throwException=true)
    {

        var logger = client.GetLogger();

        try
        {

            logger.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to create Api HttpClient");
            using (client)
            {


                if (client.BaseAddress is null)
                    throw new InvalidOperationException($"HttpClient: ({request.HttpClientName}) has a null BaseAddress");



                logger.Inspect(nameof(client.BaseAddress), client.BaseAddress);
                logger.Inspect(nameof(request.Method), request.Method);
                logger.Inspect(nameof(request.Path), request.Path);
                logger.Inspect(nameof(request.BodyContent), request.BodyContent! );



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
                innerRequest.Content = request.BodyContent;



                // *****************************************************************
                logger.Debug("Attempting to Send request");
                var innerResponse = await client.SendAsync(innerRequest, ct);

                logger.Inspect(nameof(innerResponse.StatusCode), innerResponse.StatusCode);

                if(throwException)
                    innerResponse.EnsureSuccessStatusCode();



                // *****************************************************************
                logger.Debug("Attempting to read body content");
                var content = await innerResponse.Content.ReadAsStringAsync(ct);



                // *****************************************************************
                logger.Debug("Attempting to build response");
                var response = new HttpResponse(innerResponse.StatusCode, "", innerResponse.IsSuccessStatusCode, content);


                // *****************************************************************
                return response;


            }


        }
        finally
        {
            logger.LeaveMethod();
        }


    }



}
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Models.Serialization;
using Fabrica.Watch;
using Newtonsoft.Json;

namespace Fabrica.Http;

public static class HttpClientExtensions
{


    public static async Task<HttpResponse> Send( this HttpClient client, HttpRequest request, CancellationToken token)
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
                    logger.DebugFormat("{0} = ({1})", pair.Key, pair.Value);
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
                var content = await innerResponse.Content.ReadAsStringAsync();



                // *****************************************************************
                logger.Debug("Attempting to build response");
                var response = new HttpResponse(innerResponse.StatusCode, "", true, content);


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
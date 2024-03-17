using System.Text;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Newtonsoft.Json;

namespace Fabrica.Mediator;

public class HttpRpcHandler<TRequest,TResponse>: AbstractRequestHandler<TRequest, TResponse> where TRequest : HttpRpcRequest<TResponse> where TResponse : class
{


    public HttpRpcHandler(ICorrelation correlation, IHttpClientFactory factory): base(correlation)
    {
        Factory = factory;
    }

    private IHttpClientFactory Factory { get; }

    protected override async Task<TResponse> Perform(CancellationToken token = default)
    {

        using var logger = EnterMethod();

        using var client = Factory.CreateClient( Request.HttpClientName );



        // *****************************************************************
        logger.Debug("Attempting to build BaseAddress");
        if( !string.IsNullOrWhiteSpace(Request.BaseAddress) && ! Request.BaseAddress.EndsWith("/") )
            client.BaseAddress = new Uri($"{Request.BaseAddress}/");
        else if( !string.IsNullOrWhiteSpace(Request.BaseAddress) )
            client.BaseAddress = new Uri(Request.BaseAddress);

        logger.Inspect(nameof(client.BaseAddress), client.BaseAddress);



        // *****************************************************************
        logger.Debug("Attempting to build Inner Request");
        var innerRequest = new HttpRequestMessage
        {
            Method  = Request.Method,
            RequestUri = client.BaseAddress is null ? new Uri(Request.Path) : new Uri(client.BaseAddress, Request.Path)
        };



        // *****************************************************************
        logger.Debug("Attempting to add custom headers");
        foreach (var pair in Request.CustomHeaders)
        {
            logger.Debug("{0} = ({1})", pair.Key, pair.Value);
            innerRequest.Headers.Add(pair.Key, pair.Value);
        }



        // *****************************************************************
        logger.Debug("Attempting to add body content");
        if( string.IsNullOrWhiteSpace(Request.BodyContent) )
            innerRequest.Content =  new StringContent(Request.BodyContent, Encoding.UTF8, "application/json");


        try
        {

            // *****************************************************************
            logger.Debug("Attempting to Send request");
            var innerResponse = await client.SendAsync(innerRequest, token);

            logger.Inspect(nameof(innerResponse.StatusCode), innerResponse.StatusCode);

            innerResponse.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to read body content");
            var content = await innerResponse.Content.ReadAsStringAsync(token);

            logger.LogJson("content", content );


            // *****************************************************************
            logger.Debug("Attempting to deserialize content into response");
            var response = JsonConvert.DeserializeObject<TResponse>(content);


            // *****************************************************************
            return response!;


        }
        catch (Exception cause)
        {
            logger.ErrorWithContext( cause, Request, "Perform failed during Http send");
            throw;
        }



    }


}
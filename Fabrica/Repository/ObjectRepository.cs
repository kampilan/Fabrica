using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Http;
using Fabrica.Watch;

namespace Fabrica.Repository;

public class ObjectRepository: IObjectRepository
{

    internal ObjectRepository( IHttpClientFactory factory, string repositoryClientName = "" )
    {

        Factory = factory;

        RepositoryClientName = string.IsNullOrWhiteSpace(repositoryClientName) ? Fabrica.Http.ServiceEndpoints.Repository : repositoryClientName;

    }

    private IHttpClientFactory Factory { get; }
    private string RepositoryClientName { get; }



    private async Task<RepositoryResponse> Send( RepositoryRequest request, CancellationToken token )
    {

        using var logger = this.EnterMethod();

        logger.LogObject(nameof(request), request);



        // *****************************************************************
        logger.Debug("Attempting to get Repository HttpClient");
        using var repo = Factory.CreateClient(RepositoryClientName);



        // *****************************************************************
        logger.Debug("Attempting to build HTTP request");
        var httpReq = HttpRequestBuilder.Post()
            .WithBody(request)
            .ToRequest();



        // *****************************************************************
        logger.Debug("Attempting to send HTTP request");
        var httpRes = await repo.Send(httpReq, token );

        logger.LogObject(nameof(httpRes), httpRes);



        // *****************************************************************
        logger.Debug("Attempting to build RepositoryResponse");
        var response = httpRes.FromBody<RepositoryResponse>();

        logger.LogObject(nameof(response), response);



        // *****************************************************************
        return response;


    }


    public async Task<bool> Get( Action<GetOptions> builder, CancellationToken token = default )
    {

        if (builder == null) throw new ArgumentNullException(nameof(builder));


        using var logger = this.EnterMethod();

        var ops = new GetOptions();
        builder(ops);

        logger.LogObject(nameof(ops), ops);



        // *****************************************************************
        logger.Debug("Attempting to build Repo request");
        var req = new RepositoryRequest
        {
            Key         = ops.Key,
            GenerateGet = true,
            TimeToLive  = 120
        };



        // *****************************************************************
        logger.Debug("Attempting to send Repo request");
        var res = await Send( req, token );

        if (!res.Exists)
            return false;


        // *****************************************************************
        logger.Debug("Attempting to get HttpClient for content fetch");
        using var client = Factory.CreateClient();



        // *****************************************************************
        logger.Debug("Attempting to send Get request ");
        var input = await client.GetStreamAsync( res.GetUrl );



        // *****************************************************************
        logger.Debug("Attempting to copy result to content stream");
        await input.CopyToAsync(ops.Content);


        if( !ops.Close && ops.Rewind && ops.Content.CanSeek )
            ops.Content.Seek( 0, SeekOrigin.Begin );

        if( ops.Close )
            ops.Content.Close();


        return true;


    }

    public async Task<string> Put( Action<PutOptions> builder, CancellationToken token = default)
    {

        if (builder == null) throw new ArgumentNullException(nameof(builder));


        using var logger = this.EnterMethod();


        var ops = new PutOptions();
        builder(ops);

        logger.LogObject(nameof(ops), ops);



        // *****************************************************************
        logger.Debug("Attempting to build repo request");
        var req = new RepositoryRequest
        {
            Transient     = ops.Transient,
            Key           = ops.Key,
            Extension     = ops.Extension,
            ContentType   = ops.ContentType,
            GeneratePut   = true,
            TimeToLive    = 120
        };



        // *****************************************************************
        logger.Debug("Attempting to send Repo request");
        var res = await Send(req, token);



        // *****************************************************************
        logger.Debug("Attempting to get HttpClient for content put");
        using var client = Factory.CreateClient();



        // *****************************************************************
        logger.Debug("Attempting to build StreamContent");

        if (ops.Rewind && ops.Content.CanSeek)
            ops.Content.Seek(0, SeekOrigin.Begin);

        var hc = new StreamContent(ops.Content);
        hc.Headers.ContentType = new MediaTypeHeaderValue(res.ContentType);



        // *****************************************************************
        logger.Debug("Attempting to send Put request ");
        await client.PutAsync(res.PutUrl, hc, token );



        // *****************************************************************
        return res.Key;

    }

}
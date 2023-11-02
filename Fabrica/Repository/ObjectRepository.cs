using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Http;
using Fabrica.Identity;
using Fabrica.Utilities.Text;
using Fabrica.Watch;

namespace Fabrica.Repository;

public class ObjectRepository: IObjectRepository
{

    internal ObjectRepository( IHttpClientFactory factory, string repositoryClientName = "" )
    {

        Factory     = factory;
        TokenSource = null!;

        RepositoryClientName = string.IsNullOrWhiteSpace(repositoryClientName) ? ServiceEndpoints.Repository : repositoryClientName;

    }

    internal ObjectRepository(IHttpClientFactory factory, IAccessTokenSource tokenSource, string repositoryClientName = "")
    {

        Factory = factory;
        TokenSource = tokenSource;

        RepositoryClientName = string.IsNullOrWhiteSpace(repositoryClientName) ? ServiceEndpoints.Repository : repositoryClientName;

    }


    private IHttpClientFactory Factory { get; }
    private IAccessTokenSource? TokenSource { get; }
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
        logger.Debug("Attempting to add access token to message if source is available");
        if( TokenSource is not null )
        {
            logger.Debug("Adding Access token");
            var accessToken = await TokenSource.GetToken();
            httpReq.CustomHeaders.Add("X-Fabrica-Proxy-Token", accessToken);
        }



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


    public Task<string> CreateKey( string extension="", DateTime date = default )
    {

        using var logger = this.EnterMethod();

        if( date == default )
            date = DateTime.Now;


        var ts    = date.ToUniversalTime();
        var year  = ts.Year.ToString().PadLeft(4, '0');
        var month = ts.Month.ToString().PadLeft(2, '0');
        var day   = ts.Day.ToString().PadLeft(2, '0');
        var guid  = Base62Converter.NewGuid();
        var ext   = !string.IsNullOrWhiteSpace(extension) ? extension : "";

        var key = $"{year}/{month}/{day}/{guid}";
        if( !string.IsNullOrWhiteSpace(ext) )
            key = $"{key}/document.{ext}";

        return Task.FromResult(key);


    }

    public async Task<bool> Get( Action<GetOptions> builder, CancellationToken token = default )
    {

        if (builder == null) throw new ArgumentNullException(nameof(builder));


        using var logger = this.EnterMethod();

        var ops = new GetOptions();
        builder(ops);

        logger.LogObject(nameof(ops), ops);

        if( string.IsNullOrWhiteSpace(ops.Url) )
        {

            // *****************************************************************
            logger.Debug("Attempting to build Repo request");
            var req = new RepositoryRequest
            {
                Key = ops.Key,
                GenerateGet = true,
                TimeToLive = 120
            };



            // *****************************************************************
            logger.Debug("Attempting to send Repo request");
            var res = await Send(req, token);

            if (!res.Exists)
                return false;

            ops.Url = res.GetUrl;

        }


        // *****************************************************************
        logger.Debug("Attempting to get HttpClient for content fetch");
        using var client = Factory.CreateClient();



        // *****************************************************************
        logger.Debug("Attempting to send Get request ");
        var input = await client.GetStreamAsync( ops.Url, token);



        // *****************************************************************
        logger.Debug("Attempting to copy result to content stream");
        await input.CopyToAsync(ops.Content, token);


        if( ops is {Close: false, Rewind: true, Content.CanSeek: true} )
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


        if( string.IsNullOrWhiteSpace(ops.Url) )
        {

            // *****************************************************************
            logger.Debug("Attempting to build repo request");
            var req = new RepositoryRequest
            {
                Transient = ops.Transient,
                Key = ops.Key,
                Extension = ops.Extension,
                ContentType = ops.ContentType,
                GeneratePut = true,
                TimeToLive = 120
            };



            // *****************************************************************
            logger.Debug("Attempting to send Repo request");
            var res = await Send(req, token);

            ops.Key         = res.Key;
            ops.Url         = res.PutUrl;
            ops.ContentType = res.ContentType;

        }



        // *****************************************************************
        logger.Debug("Attempting to get HttpClient for content put");
        using var client = Factory.CreateClient();



        // *****************************************************************
        logger.Debug("Attempting to build StreamContent");

        if (ops.Rewind && ops.Content.CanSeek)
            ops.Content.Seek(0, SeekOrigin.Begin);

        var hc = new StreamContent(ops.Content);
        hc.Headers.ContentType = new MediaTypeHeaderValue(ops.ContentType);



        // *****************************************************************
        logger.Debug("Attempting to send Put request ");
        await client.PutAsync(ops.Url, hc, token );



        // *****************************************************************
        return ops.Key;

    }

}
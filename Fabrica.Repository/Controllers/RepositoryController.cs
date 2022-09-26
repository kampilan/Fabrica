using Fabrica.Api.Support.Controllers;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Repository;
using Fabrica.Utilities.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Repository.Controllers;


[ApiExplorerSettings(GroupName = "Repository")]
[SwaggerResponse(200, "Success", typeof(RepositoryResponse))]
[Route("/")]
public class RepositoryController: BaseController
{

    public RepositoryController(ICorrelation correlation, IRepositoryProvider provider ) : base(correlation)
    {

        Provider = provider;

    }

    private IRepositoryProvider Provider { get; }


    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("{**catch-all}")]
    public async Task<IActionResult> Get()
    {

        using var logger = EnterMethod();


        logger.Inspect(nameof(Request.Path), Request.Path);

        if( !Request.Path.HasValue )
            return new NotFoundResult();



        // *****************************************************************
        logger.Debug("Attempting to remove leading slash if there is one");
        var key = Request.Path.ToString().StartsWith("/") ? Request.Path.ToString().Substring(1) : Request.Path.ToString();

        logger.Inspect(nameof(key), key);



        // *****************************************************************
        logger.Debug("Attempting to fetch Metadata");
        var meta = await Provider.GetMetaData(key);

        if( !meta.Exists )
            return new NotFoundResult();



        // *****************************************************************
        logger.Debug("Attempting to build Get Url");
        var url = await Provider.CreateGetUrl( key, TimeSpan.FromSeconds(120) );



        // *****************************************************************
        return new RedirectResult(url,false,true);

    }




    [SwaggerOperation(Summary = "Process Request", Description = "Process a request to get Urls to put and get documents to/from the repository")]
    [HttpPost]
    public async Task<IActionResult> Process( [FromBody] RepositoryRequest request )
    {


        using var logger = EnterMethod();

        logger.LogObject(nameof(request), request);



        // *****************************************************************
        logger.Debug("Attempting to check for leading. in Extension");
        if( !string.IsNullOrWhiteSpace(request.Extension) && request.Extension.StartsWith(".") )
            request.Extension = request.Extension[1..];



        var returnKey = false;

        // *****************************************************************
        logger.Debug("Attempting to build permanent item key");
        if( string.IsNullOrWhiteSpace(request.Key) && !request.Transient )
        {

            var ts    = DateTime.UtcNow;
            var year  = ts.Year.ToString().PadLeft(4,'0');
            var month = ts.Month.ToString().PadLeft(2,'0');
            var day   = ts.Day.ToString().PadLeft(2, '0');
            var guid  = Base62Converter.NewGuid();
            var ext   = !string.IsNullOrWhiteSpace(request.Extension) ? request.Extension : "dat";

            var key = $"{year}/{month}/{day}/{guid}/document.{ext}";

            request.Key = key;
            returnKey = true;

        }



        // *****************************************************************
        logger.Debug("Attempting to build transient item key");
        if (string.IsNullOrWhiteSpace(request.Key) && request.Transient)
        {

            var guid = Base62Converter.NewGuid();
            var ext = !string.IsNullOrWhiteSpace(request.Extension) ? request.Extension : "dat";

            var key = $"transient/{guid}/document.{ext}";

            request.Key = key;
            returnKey = true;

        }



        // *****************************************************************
        logger.Debug("Attempting to dig out Content-Type ");
        if( string.IsNullOrWhiteSpace(request.ContentType) && !string.IsNullOrWhiteSpace(request.Extension) )
        {

            var provider = new FileExtensionContentTypeProvider();

            var ext = !string.IsNullOrWhiteSpace(request.Extension) ? request.Extension : "dat";
            var fileName = $"document.{ext}";

            if( !provider.TryGetContentType(fileName, out var contentType) )
                contentType = "";

            request.ContentType = contentType;

        }


        // *****************************************************************
        logger.Debug("Attempting to sanity check TimeToLive");
        if( request.TimeToLive < 30 )
            request.TimeToLive = 30;
        else if (request.TimeToLive > 600)
            request.TimeToLive = 600;


        logger.LogObject(nameof(request), request);



        var response = new RepositoryResponse();

        if( returnKey )
            response.Key = request.Key;


        // *****************************************************************
        if( request.GenerateGet )
        {

            logger.Debug("Attempting to fetch Metadata");
            var meta = await Provider.GetMetaData( request.Key );

            response.Exists        = meta.Exists;
            response.ContentType   = meta.ContentType;
            response.ContentLength = meta.ContentLength;
            response.LastModified  = meta.LastModified;

            if( meta.Exists )
            {
                logger.Debug("Attempting to build Get Url");
                response.GetUrl = await Provider.CreateGetUrl( request.Key,TimeSpan.FromSeconds(request.TimeToLive) );
                response.Expiration = DateTime.UtcNow.AddSeconds(request.TimeToLive);
            }

        }


        // *****************************************************************
        if( request.GeneratePut )
        {
            logger.Debug("Attempting to build Get Url");
            response.PutUrl = await Provider.CreatePutUrl( request.Key, request.ContentType, TimeSpan.FromSeconds(request.TimeToLive) );
            response.Expiration = DateTime.UtcNow.AddSeconds(request.TimeToLive);
            response.ContentType = request.ContentType;
        }


        logger.LogObject(nameof(response), response);



        // *****************************************************************
        return Ok(response);


    }


}
using System.Net;
using System.Net.Mime;
using Fabrica.Mediator;
using Fabrica.Models.Serialization;
using Fabrica.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.Endpoints;

public class EndpointResult: IResult
{

    static EndpointResult()
    {

        Settings = new JsonSerializerSettings
        {
            ContractResolver           = new ModelContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling      = ReferenceLoopHandling.Serialize
        };

    }

    private static JsonSerializerSettings Settings { get; }


    public static IResult Create( object model, HttpStatusCode status = HttpStatusCode.OK )
    {

        var result = new EndpointResult
        {
            Status = status
        };

        var serializer = JsonSerializer.Create(Settings);

        using var writer = new StreamWriter(result.Output, leaveOpen: true);
        using var jwriter = new JsonTextWriter(writer);

        serializer.Serialize(jwriter, model);
        jwriter.Flush();

        result.Output.Seek(0, SeekOrigin.Begin);

        return result;

    }



    public static IResult Create<T>( Response<T> response, HttpStatusCode status=HttpStatusCode.OK )
    {

        var result = new EndpointResult
        {
            Status = status
        };

        var serializer = JsonSerializer.Create(Settings);

        if( response is {Ok: true, Value: not Stream} )
        {
            using var writer = new StreamWriter( result.Output, leaveOpen: true );
            using var jwriter = new JsonTextWriter(writer);

            serializer.Serialize( jwriter, response.Value );
            jwriter.Flush();

        }
        else if (response is { Ok: true, Value: ContentStream cs })
        {

            result.ContentType = cs.ContentType.MediaType??MediaTypeNames.Application.Json;

            if (cs.CanSeek)
                cs.Seek(0, SeekOrigin.Begin);

            cs.CopyTo(result.Output);

            cs.Dispose();

        }
        else if ( response is {Ok: true, Value: Stream stream} )
        {

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            
            stream.CopyTo(result.Output);

            stream.Dispose();

        }

        result.Output.Seek(0, SeekOrigin.Begin);

        return result;

    }

    private EndpointResult()
    {
    }

    private HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
    private string ContentType { get; set; } = MediaTypeNames.Application.Json;
    private MemoryStream Output { get; } = new();

    public async Task ExecuteAsync(HttpContext httpContext)
    {

        httpContext.Response.ContentType = ContentType;
        httpContext.Response.ContentLength = Output.Length;
        
        httpContext.Response.StatusCode = (int)Status;

        await Output.CopyToAsync(httpContext.Response.Body);

    }

}
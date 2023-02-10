﻿using System.Net;
using System.Text;
using Fabrica.Mediator;
using Fabrica.Models.Serialization;
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
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling      = ReferenceLoopHandling.Ignore
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

        using var writer = new StreamWriter(result.Output, Encoding.UTF8, leaveOpen: true);
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
            using var writer = new StreamWriter( result.Output, Encoding.UTF8, leaveOpen: true );
            using var jwriter = new JsonTextWriter(writer);

            serializer.Serialize( jwriter, response.Value );
            jwriter.Flush();

        }
        else if( response is {Ok: true, Value: Stream stream} )
        {

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            
            stream.CopyTo(result.Output);

        }

        result.Output.Seek(0, SeekOrigin.Begin);

        return result;

    }

    private EndpointResult()
    {
    }

    private HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
    private MemoryStream Output { get; } = new();

    public async Task ExecuteAsync(HttpContext httpContext)
    {

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.ContentLength = Output.Length;
        
        httpContext.Response.StatusCode = (int)Status;

        await Output.CopyToAsync(httpContext.Response.Body);

    }

}
using System.Net;
using Fabrica.Mediator;
using Fabrica.Models.Serialization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.Endpoints;

public class NewtonEndpointResultBuilder: IEndpointResultBuilder
{

    public JsonSerializerSettings Settings { get; set; } = new()
    {
        ContractResolver           = new ModelContractResolver(),
        DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
        NullValueHandling          = NullValueHandling.Ignore,
        DateTimeZoneHandling       = DateTimeZoneHandling.Utc,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ReferenceLoopHandling      = ReferenceLoopHandling.Serialize
    };


    public IResult Create( object response, HttpStatusCode status = HttpStatusCode.OK )
    {

        var result = EndpointResult.Create(response, Settings, status);

        return result;

    }

    public IResult Create<T>(Response<T> response, HttpStatusCode status = HttpStatusCode.OK)
    {

        var result = EndpointResult.Create(response, Settings, status);

        return result;

    }


}
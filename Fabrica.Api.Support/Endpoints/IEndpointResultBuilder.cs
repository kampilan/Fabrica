using Fabrica.Mediator;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Endpoints;

public interface IEndpointResultBuilder
{

    IResult Create( object response, HttpStatusCode status = HttpStatusCode.OK );

    IResult Create<T>( Response<T> response, HttpStatusCode status = HttpStatusCode.OK );

}
namespace Fabrica.Api.Support.Endpoints;

using Microsoft.AspNetCore.Routing;


public interface IEndpointModule
{

    void AddRoutes( IEndpointRouteBuilder app );

}

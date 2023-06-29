using System.Text.Json;
using System.Text.Json.Serialization;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Identity.Gateway;

public class GatewayHeaderBuilderMiddleware
{

    public GatewayHeaderBuilderMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    private RequestDelegate Next { get; }



    public async Task Invoke( HttpContext context, ICorrelation correlation, IGatewayTokenPayloadBuilder builder )
    {


        using var logger = correlation.EnterMethod<GatewayTokenBuilderMiddleware>();


        // *****************************************************************
        logger.Debug("Attempting to remove existing gateway header");
        if (context.Request.Headers.ContainsKey(IdentityConstants.IdentityHeaderName))
            context.Request.Headers.Remove(IdentityConstants.IdentityHeaderName);



        // *****************************************************************
        logger.Debug("Attempting to check if current call is authenticated");
        if (context.User.Identity is { IsAuthenticated: false })
        {
            logger.Debug("Not authenticated");
            await Next(context);
            return;
        }



        // *****************************************************************
        logger.Debug("Attempting to build claim set");
        var claims = builder.Build( context );



        // *****************************************************************
        logger.Debug("Attempting to serialize claims set to json");
        var json = JsonSerializer.Serialize( claims, claims.GetType() );



        // *****************************************************************
        logger.Debug("Attempting to set identity header");
        context.Request.Headers.Add(IdentityConstants.IdentityHeaderName, json);



        // *****************************************************************
        await Next(context);

    }


}
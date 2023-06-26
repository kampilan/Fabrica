
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Identity.Token;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Fabrica.Api.Support.Identity.Gateway;

public class GatewayTokenBuilderMiddleware
{


    public GatewayTokenBuilderMiddleware( RequestDelegate next )
    {
        Next = next;
    }

    private RequestDelegate Next { get; }

    public async Task Invoke(HttpContext context, ICorrelation correlation, IGatewayTokenPayloadBuilder builder, IGatewayTokenEncoder encoder )
    {

        using var logger = correlation.EnterMethod<GatewayTokenBuilderMiddleware>();


        // *****************************************************************
        logger.Debug("Attempting to remove existing gateway token header");
        if( context.Request.Headers.ContainsKey(IdentityConstants.TokenHeaderName) )
            context.Request.Headers.Remove(IdentityConstants.TokenHeaderName);


        // *****************************************************************
        logger.Debug("Attempting to check if current call is authenticated");
        if( context.User.Identity is {IsAuthenticated: false} )
        {
            logger.Debug("Not authenticated");
            await Next(context);
            return;
        }



        // *****************************************************************
        IClaimSet claims;
        try
        {

            logger.Debug("Attempting to build token payload");
            claims = builder.Build( context );

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Build payload failed.");
            await Next(context);
            return;
        }



        // *****************************************************************
        string token;
        try
        {

            logger.Debug("Attempting to encode token");
            token = encoder.Encode(claims);

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Encode token failed.");
            await Next(context);
            return;
        }



        // *****************************************************************
        logger.Debug("Attempting to add X-Gateway-Identity-Token header");

        context.Request.Headers[IdentityConstants.TokenHeaderName] = new StringValues(token);

        await Next(context);


    }



}
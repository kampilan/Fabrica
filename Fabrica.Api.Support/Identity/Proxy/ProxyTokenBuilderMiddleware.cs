using System;
using System.Threading.Tasks;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Fabrica.Api.Support.Identity.Proxy
{

    public class ProxyTokenBuilderMiddleware
    {


        public ProxyTokenBuilderMiddleware( RequestDelegate next )
        {
            Next = next;
        }

        private RequestDelegate Next { get; }

        public async Task Invoke(HttpContext context, ICorrelation correlation, IProxyTokenPayloadBuilder builder, IProxyTokenEncoder encoder )
        {

            using var logger = correlation.EnterMethod<ProxyTokenBuilderMiddleware>();


            // *****************************************************************
            logger.Debug("Attempting to remove existing proxy token header");
            if( context.Request.Headers.ContainsKey(TokenConstants.HeaderName) )
                context.Request.Headers.Remove(TokenConstants.HeaderName);


            // *****************************************************************
            logger.Debug("Attempting to check if current call is authenticated");
            if( !context.User.Identity.IsAuthenticated )
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
            logger.Debug("Attempting to add x-fabrica-proxy-token header");

            context.Request.Headers[TokenConstants.HeaderName] = new StringValues(token);

            await Next(context);


        }



    }


}

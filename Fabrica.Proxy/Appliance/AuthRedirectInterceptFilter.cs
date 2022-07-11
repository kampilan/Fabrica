using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Proxy.Appliance;

public class AuthRedirectInterceptFilter: IMiddleware
{

        
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {


        await next(context);


        if( context.Request.Path == "/signin-oidc" && context.Response.StatusCode == 302 )
        {
            
            var loc = context.Response.Headers["location"];
            var html = $@"<html><head><meta http-equiv='refresh' content='0;url={loc}' /></head></html>";

            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;

            await context.Response.WriteAsync(html);

        }

    }

}
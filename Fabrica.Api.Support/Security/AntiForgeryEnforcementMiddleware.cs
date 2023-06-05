using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Security;

public class AntiForgeryEnforcementMiddleware : IMiddleware
{

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {

        if (context.Request.Headers.TryGetValue("X-Gateway-Xsrf-Is-Valid", out var header))
        {

            var xsrf = header.FirstOrDefault();
            if (xsrf is not "1")
            {
                context.Response.StatusCode = 400;
                return;
            }

        }
        else
        {
            context.Response.StatusCode = 400;
            return;
        }

        await next(context);

    }

}
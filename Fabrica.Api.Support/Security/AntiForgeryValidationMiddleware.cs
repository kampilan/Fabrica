using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Security;

public class AntiForgeryValidationMiddleware : IMiddleware
{

    public AntiForgeryValidationMiddleware(IAntiforgery antiForgery)
    {
        AntiForgery = antiForgery;
    }

    private IAntiforgery AntiForgery { get; }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {

        var validated = await AntiForgery.IsRequestValidAsync(context);

        context.Request.Headers.Remove("X-Gateway-Xsrf-Is-Valid");
        context.Request.Headers.Add("X-Gateway-Xsrf-Is-Valid", validated ? "1" : "0");

        await next(context);

    }

}
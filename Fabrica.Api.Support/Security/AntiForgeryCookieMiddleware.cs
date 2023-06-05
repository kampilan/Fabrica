using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Security;

public class AntiForgeryCookieMiddleware : IMiddleware
{

    public AntiForgeryCookieMiddleware(IAntiforgery antiForgery)
    {
        AntiForgery = antiForgery;
    }

    private IAntiforgery AntiForgery { get; }


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {

        AntiForgery.GetAndStoreTokens(context);
        await next(context);

    }

}
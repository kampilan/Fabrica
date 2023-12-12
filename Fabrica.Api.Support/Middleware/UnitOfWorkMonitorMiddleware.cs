using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Middleware;

public class UnitOfWorkMonitorMiddleware
{

    public UnitOfWorkMonitorMiddleware(RequestDelegate next)
    {

        Next = next;

    }

    private RequestDelegate Next { get; }


    // ReSharper disable once UnusedMember.Global
    public async Task Invoke(HttpContext httpContext, ICorrelation correlation, IUnitOfWork? uow=null )
    {

        try
        {
            await Next(httpContext);
        }
        finally
        {

            using var logger = correlation.EnterMethod(GetType());

            uow?.Dispose();

        }

    }


}
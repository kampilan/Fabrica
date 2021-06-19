using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Api.Support.Models;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.Middleware
{


    public class PipelineMonitorMiddleware
    {

        public PipelineMonitorMiddleware(RequestDelegate next)
        {

            Next = next;

        }

        private RequestDelegate Next { get; }


        public async Task Invoke( [NotNull] HttpContext httpContext, [NotNull] ICorrelation correlation )
        {

            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (correlation == null) throw new ArgumentNullException(nameof(correlation));


            try
            {
                await Next(httpContext);
            }
            catch( Exception cause )
            {

                using( var logger = correlation.EnterMethod( GetType()) )
                {

                    logger.Error(cause, "Caught unhandled Exception in middleware pipeline");

                    logger.Inspect(nameof(httpContext.Response.HasStarted), httpContext.Response.HasStarted);

                    if (!httpContext.Response.HasStarted)
                    {

                        var em = new ErrorResponseModel
                        {
                            CorrelationId = correlation.Uid,
                            ErrorCode     = "Internal",
                            Explanation   = "Exception occurred in Middleware pipeline"
                        };


                        httpContext.Response.StatusCode  = 500;
                        httpContext.Response.ContentType = "application/json";

                        using (var stream = new MemoryStream())
                        using (var writer = new StreamWriter(stream))
                        using (var jwriter = new JsonTextWriter(writer))
                        {
                            var serializer = new JsonSerializer
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            };

                            serializer.Serialize(jwriter, em);
                            await jwriter.FlushAsync();

                            stream.Seek(0, SeekOrigin.Begin);
                            await stream.CopyToAsync(httpContext.Response.Body);

                        }

                    }


                }


            }
        }


    }


}

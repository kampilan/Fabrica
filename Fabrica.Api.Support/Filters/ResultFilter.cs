using System;
using System.Text;
using System.Threading.Tasks;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Sink;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fabrica.Api.Support.Filters
{


    public class ResultFilter: IAsyncResultFilter
    {


        public ResultFilter( ICorrelation correlation )
        {

            Correlation = correlation;
        }


        protected ICorrelation Correlation { get; }


        public virtual async Task OnResultExecutionAsync( ResultExecutingContext context, ResultExecutionDelegate next )
        {

            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            await next();

            using var logger = Correlation.EnterMethod(GetType());

            var diagLogger = Correlation.GetLogger("Fabrica.Diagnostics.Http");

            logger.Inspect(nameof(diagLogger.IsDebugEnabled), diagLogger.IsDebugEnabled);


            if( diagLogger.IsDebugEnabled )
            {

                var builder = new StringBuilder();
                builder.AppendLine( "********************************************************************************" );
                builder.AppendLine();
                builder.AppendFormat("Result Type : {0}", context.Result?.GetType().FullName ?? "");
                builder.AppendLine();

                builder.AppendFormat("Status Code : {0}", context.HttpContext.Response.StatusCode);
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine( "********************************************************************************" );

                var le = diagLogger.CreateEvent(Level.Debug, "HTTP Result", PayloadType.Text, builder.ToString());
                diagLogger.LogEvent(le);

            }
        }


    }


}

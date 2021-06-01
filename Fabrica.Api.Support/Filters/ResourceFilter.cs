using System.Threading.Tasks;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fabrica.Api.Support.Filters
{


    public class ResourceFilter: IAsyncResourceFilter
    {


        public ResourceFilter( ICorrelation correlation )
        {

            ScopeCorrelation = correlation;

        }

        private ICorrelation ScopeCorrelation { get;  }


        public async Task OnResourceExecutionAsync( ResourceExecutingContext context, [NotNull] ResourceExecutionDelegate next )
        {

            var logger = ScopeCorrelation.GetLogger(this);

            try
            {

                logger.EnterMethod();


            }
            finally
            {
                logger.LeaveMethod();
            }


            await next();


        }


    }

}

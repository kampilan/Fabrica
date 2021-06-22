using System.Runtime.CompilerServices;
using Fabrica.Watch;

namespace Fabrica.Utilities.Container
{


    public abstract class CorrelatedObject
    {


        protected CorrelatedObject( ICorrelation correlation )
        {
            Correlation = correlation;
        }


        protected ICorrelation Correlation { get; }

        protected ILogger GetLogger()
        {

            var logger = Correlation.GetLogger(this);

            return logger;

        }

        protected ILogger EnterMethod( [CallerMemberName] string name = "" )
        {

            var logger = Correlation.EnterMethod(GetType(), name);

            return logger;

        }


    }

}

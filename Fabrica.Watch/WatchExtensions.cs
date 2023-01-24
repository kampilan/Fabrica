using System.Runtime.CompilerServices;

namespace Fabrica.Watch;

public static class WatchExtensions
{


    public static ILogger GetLogger( this object target)
    {
        var logger = WatchFactoryLocator.Factory.GetLogger(target.GetType());
        return logger;
    }

    public static ILogger EnterMethod( this object target, [CallerMemberName] string name = "")
    {
        var logger = WatchFactoryLocator.Factory.GetLogger(target.GetType());
        logger.EnterMethod(name);
        return logger;
    }


}
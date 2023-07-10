using System.Runtime.CompilerServices;

namespace Fabrica.Watch;

public static class WatchExtensions
{


    public static ILogger GetLogger( this object target)
    {
        var logger = WatchFactoryLocator.Factory.GetLogger(target.GetType());
        return logger;
    }

    public static ILogger EnterMethod<T>(this T _, [CallerMemberName] string name = "")
    {
        var logger = WatchFactoryLocator.Factory.GetLogger(typeof(T).FullName!);
        logger.EnterMethod(name);
        return logger;
    }

    public static ILogger EnterMethodSlim(this object _, string category, [CallerMemberName] string name = "")
    {
        var logger = WatchFactoryLocator.Factory.GetLogger(category);
        logger.EnterMethod(name);
        return logger;
    }


}
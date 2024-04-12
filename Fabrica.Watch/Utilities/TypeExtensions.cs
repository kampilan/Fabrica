
// ReSharper disable UnusedMember.Global

namespace Fabrica.Watch.Utilities;

public static class TypeExtensions
{
    
    public static string GetConciseName(this Type type)
    {
        
        var conciseName = type.Name;
        if (!type.IsGenericType) 
            return conciseName;

        var iBacktick = conciseName.IndexOf('`');
        if (iBacktick > 0) conciseName = 
            conciseName.Remove(iBacktick);

        var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
        conciseName += "<" + string.Join(", ", genericParameters) + ">";


        return conciseName;


    }

    public static string GetConciseFullName(this Type type)
    {

        var conciseName = type.FullName;
        if (string.IsNullOrWhiteSpace(conciseName))
            return "";

        if( !type.IsGenericType )
            return conciseName;

        var iBacktick = conciseName.IndexOf('`');
        if (iBacktick > 0) conciseName =
            conciseName.Remove(iBacktick);

        var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
        conciseName += "<" + string.Join(", ", genericParameters) + ">";


        return conciseName;


    }



}
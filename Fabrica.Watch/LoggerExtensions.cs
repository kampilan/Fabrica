using Fabrica.Watch.Sink;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Watch;

public static class LoggerExtensions
{


    public static void Trace(this ILogger logger, object? message)
    {

        if (!logger.IsTraceEnabled)
            return;

        var le = logger.CreateEvent(Level.Trace, message);
        logger.LogEvent(le);

    }

    public static void Trace(this ILogger logger, Func<string> expression)
    {

        if (!logger.IsTraceEnabled)
            return;

        var le = logger.CreateEvent(Level.Trace,expression());
        logger.LogEvent(le);

    }

    public static void Trace(this ILogger logger, Exception ex, object? message=null )
    {

        if (!logger.IsTraceEnabled)
            return;

        var le = logger.CreateEvent(Level.Trace, message, ex, null);

        logger.LogEvent(le);

    }

    public static void Trace(this ILogger logger, string template, params object?[] args )
    {

        if( !logger.IsTraceEnabled )
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Trace, message);
        logger.LogEvent(le);

    }

    public static void Trace(this ILogger logger, Exception ex, string template, params object?[] args)
    {

        if (!logger.IsTraceEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent( Level.Trace, message, ex, null );

        logger.LogEvent(le);

    }


    //****************************************************************************
    public static void Debug(this ILogger logger, object? message)
    {

        if (!logger.IsDebugEnabled)
            return;

        var le = logger.CreateEvent(Level.Debug, message);
        logger.LogEvent(le);

    }

    public static void Debug(this ILogger logger, Func<string> expression)
    {

        if (!logger.IsDebugEnabled)
            return;

        var le = logger.CreateEvent(Level.Debug, expression());
        logger.LogEvent(le);

    }

    public static void Debug(this ILogger logger, Exception ex, object? message = null)
    {

        if (!logger.IsDebugEnabled)
            return;

        var le = logger.CreateEvent(Level.Debug, message, ex, null);

        logger.LogEvent(le);

    }

    public static void Debug(this ILogger logger, string template, params object?[] args)
    {

        if (!logger.IsDebugEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Debug, message);
        logger.LogEvent(le);

    }

    public static void Debug(this ILogger logger, Exception ex, string template, params object?[] args)
    {

        if (!logger.IsDebugEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Debug, message, ex, null);

        logger.LogEvent(le);

    }



    //****************************************************************************
    public static void Info(this ILogger logger, object? message)
    {

        if (!logger.IsInfoEnabled)
            return;

        var le = logger.CreateEvent(Level.Info, message);
        logger.LogEvent(le);

    }

    public static void Info(this ILogger logger, Func<string> expression)
    {

        if (!logger.IsInfoEnabled)
            return;

        var le = logger.CreateEvent(Level.Info, expression());
        logger.LogEvent(le);

    }

    public static void Info(this ILogger logger, Exception ex, object? message = null)
    {

        if (!logger.IsInfoEnabled)
            return;

        var le = logger.CreateEvent(Level.Info, message, ex, null);

        logger.LogEvent(le);

    }

    public static void Info(this ILogger logger, string template, params object?[] args)
    {

        if (!logger.IsInfoEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Info, message);
        logger.LogEvent(le);

    }

    public static void Info(this ILogger logger, Exception ex, string template, params object?[] args)
    {

        if (!logger.IsInfoEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Info, message, ex, null);

        logger.LogEvent(le);

    }





    //****************************************************************************
    public static void Warning(this ILogger logger, object? message)
    {

        if( !logger.IsWarningEnabled )
            return;

        var le = logger.CreateEvent(Level.Warning, message);
        logger.LogEvent(le);

    }

    public static void Warning(this ILogger logger, Func<string> expression)
    {

        if (!logger.IsWarningEnabled)
            return;

        var le = logger.CreateEvent(Level.Warning, expression());
        logger.LogEvent(le);

    }

    public static void Warning(this ILogger logger, Exception ex, object? message = null)
    {

        if (!logger.IsWarningEnabled)
            return;

        var le = logger.CreateEvent(Level.Warning, message, ex, null);

        logger.LogEvent(le);

    }

    public static void Warning(this ILogger logger, string template, params object?[] args)
    {

        if (!logger.IsWarningEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Warning, message);
        logger.LogEvent(le);

    }

    public static void Warning(this ILogger logger, Exception ex, string template, params object?[] args)
    {

        if (!logger.IsWarningEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Warning, message, ex, null);

        logger.LogEvent(le);

    }

    public static void WarningWithContext( this ILogger logger, Exception ex, object context, object? message = null )
    {

        if (!logger.IsWarningEnabled)
            return;

        var le = logger.CreateEvent(Level.Warning, message, ex, context);

        logger.LogEvent(le);


    }



    //****************************************************************************
    public static void Error(this ILogger logger, object? message)
    {

        if (!logger.IsErrorEnabled)
            return;

        var le = logger.CreateEvent(Level.Error, message);
        logger.LogEvent(le);

    }

    public static void Error(this ILogger logger, Func<string> expression)
    {

        if (!logger.IsErrorEnabled)
            return;

        var le = logger.CreateEvent(Level.Error, expression());
        logger.LogEvent(le);

    }

    public static void Error(this ILogger logger, Exception ex, object? message = null)
    {

        if (!logger.IsErrorEnabled)
            return;

        var le = logger.CreateEvent(Level.Error, message, ex, null);

        logger.LogEvent(le);

    }

    public static void Error(this ILogger logger, string template, params object?[] args)
    {

        if (!logger.IsErrorEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Error, message);
        logger.LogEvent(le);

    }

    public static void Error(this ILogger logger, Exception ex, string template, params object?[] args)
    {

        if (!logger.IsErrorEnabled)
            return;

        var message = string.Format(template, args);

        var le = logger.CreateEvent(Level.Error, message, ex, null);

        logger.LogEvent(le);

    }

    public static void ErrorWithContext(this ILogger logger, Exception ex, object context, object? message = null)
    {

        if (!logger.IsErrorEnabled)
            return;

        var le = logger.CreateEvent(Level.Error, message, ex, context);

        logger.LogEvent(le);


    }



    //****************************************************************************

    public static void EnterMethod(this ILogger logger, [CallerMemberName] string name = "")
    {

        if (!logger.IsDebugEnabled)
            return;

        var scope = $"{logger.Category}.{name}";
        logger.SetCurrentScope(scope);

        var le = logger.CreateEvent(Level.Debug, scope );
        le.Nesting = 1;

        logger.LogEvent(le);


    }


    public static void LeaveMethod(this ILogger logger, [CallerMemberName] string name = "")
    {

        if( !logger.IsDebugEnabled )
            return;


        logger.SetCurrentScope("");

        var scope = $"{logger.Category}.{name}";

        var le = logger.CreateEvent(Level.Debug, scope );
        le.Nesting = -1;

        logger.LogEvent(le);

        logger.Dispose();

    }



    public static void EnterScope(this ILogger logger, string name = "" )
    {

        if (!logger.IsDebugEnabled)
            return;

        var le = logger.CreateEvent(Level.Debug, name);
        le.Nesting = 1;

        logger.LogEvent(le);

        logger.SetCurrentScope(name);

    }


    public static void LeaveScope(this ILogger logger, string name = "")
    {

        if (!logger.IsDebugEnabled)
            return;

        logger.SetCurrentScope("");


        var le = logger.CreateEvent(Level.Debug, name );
        le.Nesting = -1;

        logger.LogEvent(le);

        logger.Dispose();

    }



    //****************************************************************************

    public static void Inspect( this ILogger logger, object name, object? value)
    {

        if( !logger.IsDebugEnabled )
        {
            logger.AddToRetro($"Variable: {name} = ({value??"<null>"})");
            return;
        }

        var le = logger.CreateEvent(Level.Debug, $"Variable: {name} = ({value??"<null>"})");

        logger.LogEvent(le);

    }


    //****************************************************************************

    public static void LogSql( this ILogger logger, string title, string? sql)
    {

        if (!logger.IsDebugEnabled || string.IsNullOrWhiteSpace(sql) )
            return;

        var le = logger.CreateEvent(Level.Debug, title, PayloadType.Sql, sql );

        logger.LogEvent(le);

    }

    public static void LogXml(this ILogger logger, string title, string? xml, bool pretty = true)
    {

        if ( !logger.IsDebugEnabled || string.IsNullOrWhiteSpace(xml) )
            return;

        var le = logger.CreateEvent(Level.Debug, title, PayloadType.Xml, xml );

        logger.LogEvent(le);

    }

    public static void LogJson( this ILogger logger, string title, string? json, bool pretty = true)
    {

        if (!logger.IsDebugEnabled)
            return;

        var le = logger.CreateEvent(Level.Debug, title, PayloadType.Json, json ?? "");

        logger.LogEvent(le);

    }

    public static void LogYaml( this ILogger logger, string title, string? yaml)
    {

        if (!logger.IsDebugEnabled)
            return;

        var le = logger.CreateEvent(Level.Debug, title, PayloadType.Yaml, yaml ?? "");

        logger.LogEvent(le);

    }

    public static void LogObject( this ILogger logger, string title, object? source )
    {

        if (!logger.IsDebugEnabled)
            return;

        var le = logger.CreateEvent(Level.Debug, title, source ?? new { });

        logger.LogEvent(le);

    }


}
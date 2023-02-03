
// ReSharper disable UnusedMember.Global

using System.ComponentModel;
using Fabrica.Exceptions;
using Fabrica.Models.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fabrica.Mediator;

public abstract class FluentResponse<TDescendant>: IResponse where TDescendant : FluentResponse<TDescendant>, new()
{

    public static TDescendant Build()
    {
        return new TDescendant();
    }

    public bool Ok { get; private set; }

    public virtual object GetValue() => null!;

    
    public void EnsureSuccess()
    {
        switch (Ok)
        {
            case false when InnerException is not null:
                throw InnerException;
            case false:
                throw new MediatorException(this);
        }
    }


    public Exception? InnerException { get; protected set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ErrorKind Kind { get; protected set; } = ErrorKind.System;

    [DefaultValue("")] 
    public string ErrorCode { get; protected set; } = "";

    [DefaultValue("")] 
    public string Explanation { get; protected set; } = "";

    [ExcludeEmpty]
    public List<EventDetail> Details { get; protected set; } = new ();

    public bool HasViolations => Details.Any(d => d.Category == EventDetail.EventCategory.Violation);



    public TDescendant IsOk()
    {
        Ok = true;
        return (TDescendant)this;
    }


    public TDescendant From( ExternalException ex )
    {
        WithInner(ex);
        WithKind(ex.Kind);
        WithErrorCode(ex.ErrorCode);
        WithExplanation(ex.Explanation);
        WithDetails(ex.Details);

        return (TDescendant)this;

    }

    public TDescendant WithInner(Exception inner)
    {
        InnerException = inner;
        return (TDescendant)this;
    }

    public TDescendant WithKind(ErrorKind kind)
    {

        Kind = kind;
        return (TDescendant)this;

    }

    public TDescendant WithErrorCode( string code)
    {

        ErrorCode = code ?? throw new ArgumentNullException(nameof(code));
        return (TDescendant)this;

    }

    public TDescendant WithExplanation( string explanation)
    {

        Explanation = explanation ?? throw new ArgumentNullException(nameof(explanation));
        return (TDescendant)this;

    }

    public TDescendant WithDetail( EventDetail detail)
    {

        if (detail == null) throw new ArgumentNullException(nameof(detail));

        Details.Add(detail);
        return (TDescendant)this;

    }

    public TDescendant WithDetails( IEnumerable<EventDetail> details)
    {

        if (details == null) throw new ArgumentNullException(nameof(details));

        foreach (var d in details)
            Details.Add(d);

        return (TDescendant)this;

    }

}


public class Response: FluentResponse<Response>
{

    public Response()
    {
    }

    public Response( IEnumerable<EventDetail> details )
    {
        Details.AddRange(details);
    }


}


public class Response<TValue>: FluentResponse<Response<TValue>>
{

    public static implicit operator TValue(Response<TValue> response) => response.Value;

    public Response()
    {
        Value = default!;
    }

    public Response( TValue value, IEnumerable<EventDetail>? details=null )
    {

        Value = value;

        if( details != null )
            Details.AddRange( details);

    }

    public TValue Value { get; }

    public override object GetValue() => Value!;

}
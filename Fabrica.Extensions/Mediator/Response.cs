using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fabrica.Exceptions;
using Fabrica.Models.Serialization;
using JetBrains.Annotations;
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

    public virtual object GetValue() => null;
    public void EnsureSuccess()
    {
        if( !Ok )
            throw new MediatorException(this);
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public ErrorKind Kind { get; protected set; } = ErrorKind.System;

    [DefaultValue("")] 
    public string ErrorCode { get; protected set; } = "";

    [DefaultValue("")] 
    public string Explanation { get; protected set; } = "";

    [ExcludeEmpty]
    public List<EventDetail> Details { get; protected set; } = new ();

    public bool HasViolations => Details.Any(d => d.Category == EventDetail.EventCategory.Violation);


    [NotNull]
    public TDescendant IsOk()
    {
        Ok = true;
        return (TDescendant)this;
    }

    [NotNull]
    public TDescendant From( ExternalException ex )
    {

        WithKind(ex.Kind);
        WithErrorCode(ex.ErrorCode);
        WithExplaination(ex.Explanation);
        WithDetails(ex.Details);

        return (TDescendant)this;

    }

    [NotNull]
    public TDescendant WithKind(ErrorKind kind)
    {

        Kind = kind;
        return (TDescendant)this;

    }

    [NotNull]
    public TDescendant WithErrorCode([NotNull] string code)
    {

        ErrorCode = code ?? throw new ArgumentNullException(nameof(code));
        return (TDescendant)this;

    }

    [NotNull]
    public TDescendant WithExplaination([NotNull] string explaination)
    {

        Explanation = explaination ?? throw new ArgumentNullException(nameof(explaination));
        return (TDescendant)this;

    }

    [NotNull]
    public TDescendant WithDetail([NotNull] EventDetail detail)
    {

        if (detail == null) throw new ArgumentNullException(nameof(detail));

        Details.Add(detail);
        return (TDescendant)this;

    }

    [NotNull]
    public TDescendant WithDetails([NotNull] IEnumerable<EventDetail> details)
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

    public Response()
    {
    }

    public Response( TValue value, IEnumerable<EventDetail> details=null )
    {

        Value = value;

        if( details != null )
            Details.AddRange( details);

    }

    public TValue Value { get; }

    public override object GetValue() => Value;

}
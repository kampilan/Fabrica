using Fabrica.Exceptions;
using System.Text.Json.Serialization;
using static Fabrica.Exceptions.EventDetail;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Utilities.Types;

public class Envelope<TValue> where TValue : class
{

    public static implicit operator TValue?(Envelope<TValue> res) => res.Value;


    public bool Ok { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorKind Kind { get; set; }

    public string ErrorCode { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;

    public List<EnvelopeDetail> Details { get; set; } = new();

    public bool HasViolations => Details.Any(d => d.Category == EventDetail.EventCategory.Violation);

    public TValue? Value { get; set; }


}


public class Envelope
{

    public bool Ok { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorKind Kind { get; set; }

    public string ErrorCode { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;

    public List<EnvelopeDetail> Details { get; set; } = new();

    public bool HasViolations => Details.Any(d => d.Category == EventDetail.EventCategory.Violation);

}


public class EnvelopeDetail
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EventCategory Category { get; set; } = EventCategory.Error;

    public string RuleName { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;


}
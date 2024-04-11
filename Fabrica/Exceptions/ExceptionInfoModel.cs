using System.ComponentModel;
using System.Text.Json.Serialization;
using Fabrica.Models.Serialization;

namespace Fabrica.Exceptions;

public class ExceptionInfoModel: IExceptionInfo
{


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorKind Kind { get; set; } = ErrorKind.Unknown;

    [DefaultValue("")]
    public string ErrorCode { get; set; } = "";

    [DefaultValue("")]
    public string Explanation { get; set; } = "";

    [ExcludeEmpty]
    public List<EventDetail> Details { get; set; } = new ();


}
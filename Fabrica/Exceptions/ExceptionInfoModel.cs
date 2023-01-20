using System.ComponentModel;
using Fabrica.Models.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fabrica.Exceptions;

public class ExceptionInfoModel: IExceptionInfo
{


    [JsonConverter(typeof(StringEnumConverter))]
    public ErrorKind Kind { get; set; } = ErrorKind.Unknown;

    [DefaultValue("")]
    public string ErrorCode { get; set; } = "";

    [DefaultValue("")]
    public string Explanation { get; set; } = "";

    [ExcludeEmpty]
    public List<EventDetail> Details { get; set; } = new ();


}
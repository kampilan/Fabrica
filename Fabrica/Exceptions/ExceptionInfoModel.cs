using System.Collections.Generic;

namespace Fabrica.Exceptions
{


    public class ExceptionInfoModel: IExceptionInfo
    {

        public ErrorKind Kind { get; set; } = ErrorKind.Unknown;

        public string ErrorCode { get; set; } = "";
        public string Explanation { get; set; } = "";

        public List<EventDetail> Details { get; set; } = new List<EventDetail>();


    }


}

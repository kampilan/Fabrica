namespace Fabrica.Exceptions;

public interface IExceptionInfo
{

    ErrorKind Kind { get; }
    string ErrorCode { get; }
    string Explanation { get; }

    List<EventDetail> Details { get; }

}
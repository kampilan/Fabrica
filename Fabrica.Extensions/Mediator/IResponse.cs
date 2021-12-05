using Fabrica.Exceptions;

namespace Fabrica.Mediator;

public interface IResponse: IExceptionInfo
{

    bool Ok { get; }

    object GetValue();

    void EnsureSuccess();


}
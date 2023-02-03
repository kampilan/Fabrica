using Fabrica.Exceptions;

namespace Fabrica.Mediator;

public class MediatorException: FluentException<MediatorException>
{


    public MediatorException( IExceptionInfo response ): base( $"Request failed: ({response.ErrorCode} - {response.Explanation})" )
    {

        WithKind(response.Kind);
        WithErrorCode(response.ErrorCode);
        WithExplanation(response.Explanation);
        WithDetails(response.Details);

    }

    public MediatorException(IExceptionInfo response, Exception inner) : base($"Request failed: ({response.ErrorCode} - {response.Explanation})", inner)
    {

        WithKind(response.Kind);
        WithErrorCode(response.ErrorCode);
        WithExplanation(response.Explanation);
        WithDetails(response.Details);

    }



    public MediatorException( string message ) : base( message )
    {
    }

    public MediatorException( string message, Exception inner) : base( message, inner )
    {
    }

}
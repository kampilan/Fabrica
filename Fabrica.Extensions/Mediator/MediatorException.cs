using System;
using Fabrica.Exceptions;

namespace Fabrica.Mediator
{

    
    public class MediatorException: FluentException<MediatorException>
    {


        public MediatorException( IResponse response ): base( $"Request failed: ({response.ErrorCode} - {response.Explanation})" )
        {

            WithKind(response.Kind);
            WithErrorCode(response.ErrorCode);
            WithExplaination(response.Explanation);
            WithDetails(response.Details);

        }

        public MediatorException( string message ) : base( message )
        {
        }

        public MediatorException( string message, Exception inner) : base( message, inner )
        {
        }

    }


}

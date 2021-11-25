using System;
using System.Collections.Generic;
using Fabrica.Exceptions;

namespace Fabrica.Mediator
{


    public class MediatorInvalidRequestException: FluentException<MediatorInvalidRequestException>
    {


        public MediatorInvalidRequestException( IEnumerable<EventDetail> details ) : base("Mediator encountered a invalid request failed")
        {

            WithKind(ErrorKind.BadRequest);
            WithErrorCode( GetType().FullName??"" );
            WithExplaination("This request is invalid. Examine details for a full explanation." );
            WithDetails(details);

        }
      


        public MediatorInvalidRequestException(string message) : base(message)
        {
        }

        public MediatorInvalidRequestException(string message, Exception inner) : base(message, inner)
        {
        }


    }

}

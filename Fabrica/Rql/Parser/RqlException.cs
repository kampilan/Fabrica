using System;
using Fabrica.Exceptions;

namespace Fabrica.Rql.Parser
{


    public class RqlException: FluentException<RqlException>
    {

        public RqlException(string message) : base(message)
        {
        }

        public RqlException(string message, Exception inner) : base(message, inner)
        {
        }

    }


}

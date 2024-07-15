
namespace Fabrica.Rql.Parser;

public class RqlException: Exception
{

    public RqlException(string message) : base(message)
    {
    }

    public RqlException(string message, Exception inner) : base(message, inner)
    {
    }

}
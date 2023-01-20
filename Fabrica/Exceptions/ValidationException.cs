namespace Fabrica.Exceptions;

public class ValidationException: PredicateException
{

    public ValidationException( IEnumerable<EventDetail> details) : base( "Validation errors exist" )
    {

        WithDetails(details);

    }

    public ValidationException( string explanation, IEnumerable<EventDetail> details) : base( "Validation errors exist" )
    {

        WithExplanation(explanation);
        WithDetails(details);

    }

}
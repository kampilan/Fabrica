namespace Fabrica.Utilities.Types
{

    
    public interface IDateTimeRange
    {

        int Id { get; }

        DateTimeRange RangeKind { get; }

        string Label { get; }

        DateTime Begin { get; }

        DateTime End { get; }

    }


}

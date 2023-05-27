namespace Fabrica.Utilities.Types;

public class DateTimeRangeModel: IDateTimeRange
{


    public int Id { get; set; }

    public string Label { get; set; } = "Today";

    public DateTimeRange RangeKind { get; set; } = DateTimeRange.Today;

    public DateTime Begin => DateTimeHelpers.CalculateRange(RangeKind).begin;
    public DateTime End => DateTimeHelpers.CalculateRange(RangeKind).end;


    int IDateTimeRange.Id => Id;
    string IDateTimeRange.Label => Label;
    DateTime IDateTimeRange.Begin => Begin;
    DateTime IDateTimeRange.End => End;


}
namespace Fabrica.Watch.Sink;

public class LogViewerEvent
{

    public LogViewerEvent()
    {

    }

    public LogViewerEvent( ILogEvent source )
    {

        Tenant        = source.Tenant;
        Subject       = source.Subject;
        Tag           = source.Tag;
        Category      = source.Category;
        CorrelationId = source.CorrelationId;
        Nesting       = source.Nesting;
        Color         = source.Color;
        Level         = (int) source.Level;
        Title         = source.Title;
        Occurred      = source.Occurred;
        Type          = source.Type;
        Payload       = source.Payload;

    }


    public string Tenant { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Tag { get; set; } = "";


    public string Category { get; set; } = "";
    public string CorrelationId { get; set; } = "";

    public int Nesting { get; set; }
    public int Color   { get; set; }
    public int Level   { get; set; }
    public string Title   { get; set; } = "";

    public DateTime Occurred { get; set; } = DateTime.UtcNow;
    public PayloadType Type { get; set; } = PayloadType.None;
    public string Payload  { get; set; } = "";


    public int ParentId { get; set; } = 0;
    public int NodeId { get; set; } = 0;
    public int ImageId { get; set; } = 0;
    public TimeSpan Offset { get; set; } = TimeSpan.MinValue;
    public TimeSpan Diff { get; set; } = TimeSpan.MinValue;


}
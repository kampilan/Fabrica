using System.Drawing;

namespace Fabrica.Watch;

public class LoggerRequest
{

    public bool Debug { get; set; } = false;
    public Level Level { get; set; } = Level.Debug;
    public Color Color { get; set; } = Color.PapayaWhip;

    public string Tenant { get; set; } = "";
    public string Subject { get; set; } = "";

    public string Category { get; set; } = "";
    public string CorrelationId { get; set; } = "";


    public IList<(string Key, string Target)> FilterKeys { get; } = new List<(string Key, string Target)>();


}
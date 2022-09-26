namespace Fabrica.Work.Models;

public class IngestionEvent
{

    public string Endpoint { get; set; } = "";

    public string ContentType { get; set; } = "";
    public long Size { get; set; }

}
namespace Fabrica.Work.Models;

public class S3CreateEvent
{

    public string Region { get; set; } = "";
    public string Bucket { get; set; } = "";
    public string Key { get; set; } = "";
    public long Size { get; set; }

    public string Operation { get; set; } = "";
    public string Timestamp { get; set; } = "";


}
namespace Fabrica.Repository;

public class RepositoryRequest
{

    public bool Transient { get; set; }

    public string Key { get; set; } = "";

    public int TimeToLive { get; set; }

    public bool GenerateGet { get; set; }

    public bool GeneratePut { get; set; }
    public string Extension { get; set; } = "";
    public string ContentType { get; set; } = "";


}
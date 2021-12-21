namespace Fabrica.Persistence.Connection;

public class DbCredentials
{

    public string OriginDbUserName { get; set; } = "";
    public string OriginDbPassword { get; set; } = "";

    public string ReplicaDbUserName { get; set; } = "";
    public string ReplicaDbPassword { get; set; } = "";

}
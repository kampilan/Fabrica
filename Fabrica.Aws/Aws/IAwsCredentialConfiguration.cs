namespace Fabrica.Aws;

public interface IAwsCredentialConfiguration
{

    string Profile { get; set; }

    // ReSharper disable once InconsistentNaming
    bool RunningOnEC2 { get; set; }

}
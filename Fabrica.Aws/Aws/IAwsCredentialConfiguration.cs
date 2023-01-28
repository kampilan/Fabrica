namespace Fabrica.Aws;

public interface IAwsCredentialConfiguration
{

    string AwsProfileName { get; set; }

    // ReSharper disable once InconsistentNaming
    bool UseLocalAwsCredentials { get; set; }

}
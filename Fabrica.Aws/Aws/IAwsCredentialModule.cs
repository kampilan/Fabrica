namespace Fabrica.Aws
{

    public interface IAwsCredentialModule
    {

        string Profile { get; set; }


        string RegionName { get; set; }
        string AccessKey { get; set; }
        string SecretKey { get; set; }

        // ReSharper disable once InconsistentNaming
        bool RunningOnEC2 { get; set; }

    }

}

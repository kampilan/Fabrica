using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Fabrica.Watch;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Aws;

public static class StsExtensions
{


    public static async Task<CredentialSet> CreateCredentialSet(this IAmazonSecurityTokenService service, string arn, string uid, string policy = "", TimeSpan duration=default)
    {

        if (string.IsNullOrWhiteSpace(arn)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(arn));
        if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

        using var logger = service.GetLogger();


        logger.Inspect(nameof(arn), arn);
        logger.Inspect(nameof(uid), uid);
        logger.Inspect(nameof(policy), policy);


        duration = duration == default || duration.TotalSeconds < 900 ? TimeSpan.FromSeconds(900) : duration;
        var durSeconds = Convert.ToInt32(duration.TotalSeconds);

        logger.Inspect(nameof(durSeconds), durSeconds);


        var request = new AssumeRoleRequest
        {
            RoleArn = arn,
            RoleSessionName = uid,
            DurationSeconds = durSeconds
        };

        if( !string.IsNullOrWhiteSpace(policy) )
            request.Policy = policy;



        // *****************************************************************
        logger.Debug("Attempting to call AssumeRole");
        var response = await service.AssumeRoleAsync(request);



        // *****************************************************************
        logger.Debug("Attempting to build CredentialSet");
        var set = new CredentialSet
        {
            AccessKey    = response.Credentials.AccessKeyId,
            SecretKey    = response.Credentials.SecretAccessKey,
            SessionToken = response.Credentials.SessionToken,
            Expiration   = response.Credentials.Expiration
        };



        // *****************************************************************
        return set;


    }


}

public class CredentialSet
{

    public string AccessKey { get; set; } = "";

    public string SecretKey { get; set; } = "";

    public string SessionToken { get; set; } = "";

    public DateTime Expiration { get; set; }

}
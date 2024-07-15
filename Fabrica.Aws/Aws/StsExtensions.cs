using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Aws;

public static class StsExtensions
{


    public static async Task<CredentialSet> CreateCredentialSet(this IAmazonSecurityTokenService service, string arn, string uid, string policy = "", TimeSpan duration=default)
    {

        if (string.IsNullOrWhiteSpace(arn)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(arn));
        if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));



        duration = duration == default || duration.TotalSeconds < 900 ? TimeSpan.FromSeconds(900) : duration;
        var durSeconds = Convert.ToInt32(duration.TotalSeconds);


        var request = new AssumeRoleRequest
        {
            RoleArn = arn,
            RoleSessionName = uid,
            DurationSeconds = durSeconds
        };

        if( !string.IsNullOrWhiteSpace(policy) )
            request.Policy = policy;



        // *****************************************************************
        var response = await service.AssumeRoleAsync(request);



        // *****************************************************************
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

public class StsConfiguration
{

    public string RoleArn { get; set; } = string.Empty;
    public string Policy { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; } = default;

}


public class CredentialSet
{

    public string Region{ get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    public string SessionToken { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

}
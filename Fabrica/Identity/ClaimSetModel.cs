using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fabrica.Identity;

public class ClaimSetModel: IClaimSet
{

    [JsonPropertyName("aty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AuthenticationType { get; set; }

    [JsonPropertyName("flw")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AuthenticationFlow { get; set; }


    [JsonPropertyName("exp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Expiration { get; set; }
    public void SetExpiration(TimeSpan ttl)
    {
        var exp = DateTime.UtcNow + ttl;
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        Expiration = Convert.ToInt64((exp - epoch).TotalSeconds);
    }

    [JsonPropertyName("ten")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Tenant { get; set; }

    [JsonPropertyName("sub")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Subject { get; set; }

    [JsonPropertyName("nam")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }

    [JsonPropertyName("eml")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Email { get; set; }

    [JsonPropertyName("pic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Picture { get; set; }

    [JsonPropertyName("rol")]
    public List<string> Roles { get; set; } = new ();

    IEnumerable<string> IClaimSet.Roles => Roles;


}
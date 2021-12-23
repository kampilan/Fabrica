using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fabrica.Identity;

[JsonObject(MemberSerialization.OptIn)]
public class ClaimSetModel: IClaimSet
{

    [JsonProperty("aty")]
    public string AuthenticationType { get; set; }

    [JsonProperty("exp")]
    public long? Expiration { get; set; }
    public void SetExpiration(TimeSpan ttl)
    {
        var exp = DateTime.UtcNow + ttl;
        Expiration = Convert.ToInt64((exp - DateTime.UnixEpoch).TotalSeconds);
    }

    [JsonProperty("ten")]
    public string Tenant { get; set; }

    [JsonProperty("sub")]
    public string Subject { get; set; }

    [JsonProperty("nam")]
    public string Name { get; set; }

    [JsonProperty("eml")]
    public string Email { get; set; }

    [JsonProperty("pic")]
    public string Picture { get; set; }

    public List<string> Roles { get; set; } = new ();

    [JsonProperty("rol")]
    IEnumerable<string> IClaimSet.Roles => Roles;


}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Fabrica.Identity;

[JsonObject(MemberSerialization.OptIn)]
public class ClaimSetModel: IClaimSet
{

    [JsonProperty("aty")]
    [DefaultValue("")]
    public string AuthenticationType { get; set; } = "";

    [JsonProperty("flw")]
    [DefaultValue("")]
    public string AuthenticationFlow { get; set; } = "";


    [JsonProperty("exp")]
    [DefaultValue(0L)]
    public long? Expiration { get; set; } = 0;
    public void SetExpiration(TimeSpan ttl)
    {
        var exp = DateTime.UtcNow + ttl;
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        Expiration = Convert.ToInt64((exp - epoch).TotalSeconds);
    }

    [JsonProperty("ten")]
    [DefaultValue("")]
    public string Tenant { get; set; } = "";

    [JsonProperty("sub")]
    [DefaultValue("")]
    public string Subject { get; set; } = "";

    [JsonProperty("nam")]
    [DefaultValue("")]
    public string Name { get; set; } = "";

    [JsonProperty("eml")]
    [DefaultValue("")]
    public string Email { get; set; } = "";

    [JsonProperty("pic")]
    [DefaultValue("")]
    public string Picture { get; set; } = "";

    public List<string> Roles { get; set; } = new ();

    [JsonProperty("rol")]
    IEnumerable<string> IClaimSet.Roles => Roles;


}
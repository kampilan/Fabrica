﻿using System.Text.Json.Serialization;

namespace Fabrica.Identity.Keycloak.Models;

public class UserConsent
{

    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }
    [JsonPropertyName("grantedClientScopes")]
    public ICollection<string>? GrantedClientScopes { get; set; }
    [JsonPropertyName("createdDate")]
    public long? CreatedDate { get; set; }
    [JsonPropertyName("lastUpdatedDate")]
    public long? LastUpdatedDate { get; set; }


}


﻿using System.Text.Json.Serialization;

namespace Fabrica.Identity.Keycloak.Models;

public class UserAccess
{

    [JsonPropertyName("manageGroupMembership")]
    public bool? ManageGroupMembership { get; set; }
    [JsonPropertyName("view")]
    public bool? View { get; set; }
    [JsonPropertyName("mapRoles")]
    public bool? MapRoles { get; set; }
    [JsonPropertyName("impersonate")]
    public bool? Impersonate { get; set; }
    [JsonPropertyName("manage")]
    public bool? Manage { get; set; }


}
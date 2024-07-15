using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Identity.Keycloak.Models;

public class User
{

    [JsonPropertyName("self")]
    public string? Self { get; set; }
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("origin")]
    public string? Origin { get; set; }
    [JsonPropertyName("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }
    [JsonPropertyName("username")]
    public string? UserName { get; set; }
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    [JsonPropertyName("totp")]
    public bool? Totp { get; set; }
    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("federationLink")]
    public string? FederationLink { get; set; }
    [JsonPropertyName("serviceAccountClientId")]
    public string? ServiceAccountClientId { get; set; }
    [JsonPropertyName("attributes")]
    public Dictionary<string, IEnumerable<string>>? Attributes { get; set; }
    [JsonPropertyName("credentials")]
    public ICollection<Credentials>? Credentials { get; set; }
    [JsonPropertyName("disableableCredentialTypes")]
    public ICollection<string>? DisableableCredentialTypes { get; set; }
    [JsonPropertyName("requiredActions")]
    public ICollection<string>? RequiredActions { get; set; }
    [JsonPropertyName("federatedIdentities")]
    public ICollection<FederatedIdentity>? FederatedIdentities { get; set; }
    [JsonPropertyName("realmRoles")]
    public ICollection<string>? RealmRoles { get; set; }
    [JsonPropertyName("clientRoles")]
    public IDictionary<string, object>? ClientRoles { get; set; }
    [JsonPropertyName("clientConsents")]
    public ICollection<UserConsent>? ClientConsents { get; set; }
    [JsonPropertyName("notBefore")]
    public int? NotBefore { get; set; }
    [JsonPropertyName("applicationRoles")]
    public IDictionary<string, object>? ApplicationRoles { get; set; }
    [JsonPropertyName("socialLinks")]
    public ICollection<SocialLink>? SocialLinks { get; set; }
    [JsonPropertyName("groups")]
    public ICollection<string>? Groups { get; set; }
    [JsonPropertyName("access")]
    public IDictionary<string, bool>? Access { get; set; }
    [JsonPropertyName("userProfileMetadata")]
    public UserProfileMetadata? UserProfileMetadata { get; set; }

}
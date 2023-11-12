using System.Text.Json.Serialization;

namespace Fabrica.Identity.Keycloak.Models;

public class Credentials
{

    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("userLabel")]
    public string? UserLabel { get; set; }
    [JsonPropertyName("createdDate")]
    public long? CreatedDate { get; set; }
    [JsonPropertyName("secretData")]
    public string? SecretData { get; set; }
    [JsonPropertyName("credentialData")]
    public string? CredentialData { get; set; }
    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
    [JsonPropertyName("value")]
    public string? Value { get; set; }
    [JsonPropertyName("temporary")]
    public bool? Temporary { get; set; }
    [JsonPropertyName("device")]
    public string? Device { get; set; }
    [JsonPropertyName("hashSaltedValue")]
    public string? HashSaltedValue { get; set; }
    [JsonPropertyName("salt")]
    public string? Salt { get; set; }
    [JsonPropertyName("hashIterations")]
    public int? HashIterations { get; set; }
    [JsonPropertyName("counter")]
    public int? Counter { get; set; }
    [JsonPropertyName("algorithm")]
    public string? Algorithm { get; set; }
    [JsonPropertyName("digits")]
    public int? Digits { get; set; }
    [JsonPropertyName("period")]
    public int? Period { get; set; }
    [JsonPropertyName("config")]
    public IDictionary<string, string>? Config { get; set; }


}
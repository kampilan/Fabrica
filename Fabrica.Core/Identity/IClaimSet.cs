﻿namespace Fabrica.Identity;

public interface IClaimSet
{

    string? AuthenticationType { get; }

    string? AuthenticationFlow { get; }

    long? Expiration { get; }

    void SetExpiration(TimeSpan ttl);

    string? Tenant { get; }

    string? Subject { get; }

    string? AltSubject { get; }

    string? UserName { get; }

    string? GivenName { get; }

    string? FamilyName { get; }

    string? Name { get; }

    string? Email { get; }

    string? Picture { get; }

    public IEnumerable<string> Roles { get;}


}
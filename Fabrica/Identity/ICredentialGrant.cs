﻿namespace Fabrica.Identity;

public interface ICredentialGrant
{

    string Name { get; }

    string MetaEndpoint { get; }
    string TokenEndpoint { get; }

    IReadOnlyDictionary<string, string> Body { get; }

}
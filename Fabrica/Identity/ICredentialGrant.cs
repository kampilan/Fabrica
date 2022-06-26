using System.Collections.Generic;

namespace Fabrica.Identity;

public interface ICredentialGrant
{

    string Name { get; }

    string MetaEndpoint { get; }
    string TokenEndpoint { get; }

    IDictionary<string, string> Body { get; }

}
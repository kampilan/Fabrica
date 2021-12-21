using System.Collections.Generic;

namespace Fabrica.Identity
{

    public interface ICredentialGrant
    {

        IDictionary<string, string> Body { get; }

    }
}

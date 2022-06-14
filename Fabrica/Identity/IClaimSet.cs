using System;
using System.Collections.Generic;

namespace Fabrica.Identity
{


    public interface IClaimSet
    {

        string AuthenticationType { get; }

        string AuthenticationFlow { get; }

        long? Expiration { get; }

        void SetExpiration(TimeSpan ttl);

        string Tenant { get; }

        string Subject { get; }

        string Name { get; }

        string Email { get; }

        string Picture { get; }

        public IEnumerable<string> Roles { get;}


    }


}
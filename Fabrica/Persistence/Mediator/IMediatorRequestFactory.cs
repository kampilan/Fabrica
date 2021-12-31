using System;
using System.Collections.Generic;

namespace Fabrica.Persistence.Mediator
{

    public interface IMediatorRequestFactory
    {

        public ICreateEntityRequest GetCreateRequest( Type entity, string uid, IDictionary<string,object> delta );
        public ICreateMemberEntityRequest GetCreateMemberRequest(Type parent, string parentUid, Type member, string uid, IDictionary<string, object> delta);
        public IUpdateEntityRequest GetUpdateRequest(Type entity, string uid, IDictionary<string, object> delta);
        public IDeleteEntityRequest GetDeleteRequest(Type entity, string uid);

    }

}

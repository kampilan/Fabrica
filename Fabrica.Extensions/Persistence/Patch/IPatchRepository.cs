using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fabrica.Persistence.Patch
{

    public interface IPatchRepository
    {

        Task<object> HandleRetrieve( Type reference, string uid );

        Task<object> HandleCreate( Type entity, string uid, IDictionary<string, object> properties );

        Task<object> HandleUpdate( Type entity, string uid, IDictionary<string, object> properties );

        Task HandleDelete( Type entity, string uid );

        Task Save();
        Task Abort();

    }

}

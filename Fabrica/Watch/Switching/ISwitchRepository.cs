using System.Collections.Generic;

namespace Fabrica.Watch.Switching
{


    public interface ISwitchRepository
    {

        ISwitchEntity Create();

        IEnumerable<ISwitchEntity> Retrieve();

        void Update( IEnumerable<ISwitchEntity> updates );

        void Delete( IEnumerable<ISwitchEntity> deletes );

    }


}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.One.Models;

namespace Fabrica.One.Repository
{


    public interface IRepository
    {

        Task<IEnumerable<MissionModel>> GetMissions( Func<MissionModel,bool> predicate=null );
        Task<IEnumerable<BuildModel>> GetBuilds( Func<BuildModel,bool> predicate = null );

        Task Reload();


        Task<MissionModel> CreateMission( string name );

        Task Save( MissionModel mission );

        Task Delete( MissionModel model );


    }


}

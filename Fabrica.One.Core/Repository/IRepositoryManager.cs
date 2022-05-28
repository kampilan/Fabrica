using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.One.Models;

namespace Fabrica.One.Repository
{


    public interface IRepositoryManager
    {

        Task Refresh();


        RepositoryModel CurrentRepository { get; }
        Task SetCurrentRepository(RepositoryModel current);


        Task<IEnumerable<RepositoryModel>> GetRepositories(Func<RepositoryModel, bool> predicate = null);
        Task<IEnumerable<MissionModel>> GetMissions( Func<MissionModel,bool> predicate=null );
        Task<IEnumerable<ApplianceModel>> GetAppliances(Func<ApplianceModel,bool> predicate = null);
        Task<IEnumerable<BuildModel>> GetBuilds( Func<BuildModel,bool> predicate = null );


        Task<MissionModel> CreateMission( string name, string environment, string customName="" );

        Task Save( MissionModel mission );

        Task Deploy( MissionModel model, string version="1" );

        Task Delete( MissionModel model );


    }


}

using System;
using System.Threading.Tasks;
using Fabrica.Utilities.Repository;

namespace Fabrica.Repository;


public interface IRepositoryProvider
{

    Task<RepositoryObjectMeta> GetMetaData( string key );

    Task<string> CreateGetUrl(  string key, TimeSpan ttl=default );
    
    Task<string> CreatePutUrl(  string key, string contentType = "", TimeSpan ttl = default);

    Task Move( string sourceKey, string destinationKey );

    Task Delete( string key );

}
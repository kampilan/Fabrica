using System;
using System.Threading.Tasks;

namespace Fabrica.Utilities.Repository;


public interface IRepositoryUrlProvider
{

    Task<RepositoryObjectMeta> GetMetaData( string key );

    Task<string> CreateGetUrl(  string key, TimeSpan ttl=default );
    
    Task<string> CreatePutUrl(  string key, string contentType = "", TimeSpan ttl = default);


}
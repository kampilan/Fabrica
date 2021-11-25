using System;

namespace Fabrica.Utilities.Repository
{


    public enum RepositoryType {Transient, Permanent, Resource }


    public interface IRepositoryUrlProvider
    {

        string CreateGetUrl( string repositoryName, string key, string contentType="", TimeSpan ttl=default );
        string CreateGetUrl( RepositoryType type, string key, string contentType = "", TimeSpan ttl = default);

        string CreatePutUrl( string repositoryName, string key, string contentType = "", TimeSpan ttl = default);
        string CreatePutUrl( RepositoryType type, string key, string contentType = "", TimeSpan ttl = default);


    }

}

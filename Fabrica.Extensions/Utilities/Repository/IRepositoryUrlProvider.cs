using System;
using System.Threading.Tasks;

namespace Fabrica.Utilities.Repository;


public interface IRepositoryUrlProvider
{

    Task<RepositoryObjectMeta> GetMetaData( string key );

    Task<string> CreateGetUrl(  string key, TimeSpan ttl=default );
    
    Task<string> CreatePutUrl(  string key, string contentType = "", TimeSpan ttl = default);


}


public class RepositoryObjectMeta
{

    public RepositoryObjectMeta(bool exists, string contentType, long contentLength, DateTime lastModified)
    {

        Exists       = exists;
        ContentType   = contentType;
        ContentLength = contentLength;
        LastModified  = lastModified;
    }

    public bool Exists { get; }

    public string ContentType { get; }
    public long ContentLength { get; }

    public DateTime LastModified { get; }

}
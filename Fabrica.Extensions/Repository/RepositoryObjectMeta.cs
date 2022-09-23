using System;

namespace Fabrica.Utilities.Repository;

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
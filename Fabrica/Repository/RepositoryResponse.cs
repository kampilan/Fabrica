using System;
using System.ComponentModel;

namespace Fabrica.Repository;

public class RepositoryResponse
{

    [DefaultValue("")]
    public string Key { get; set; } = "";

    [DefaultValue(0)]
    public bool Exists { get; set; }

    [DefaultValue("")]
    public string ContentType { get; set; } = "";
    [DefaultValue(0)]
    public long ContentLength { get; set; }

    public DateTime LastModified { get; set; }


    public DateTime Expiration { get; set; }
    
    [DefaultValue("")]
    public string GetUrl { get; set; } = "";
    [DefaultValue("")] 
    public string PutUrl { get; set; } = "";


}
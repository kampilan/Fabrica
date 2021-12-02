using System.Collections.Generic;

namespace Fabrica.Models.Support;

public  interface IApiModel
{

    public bool IsOverposted();

    public IEnumerable<string> GetOverpostNames();

}
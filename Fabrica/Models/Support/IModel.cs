using System.Collections.Generic;
using System.ComponentModel;
using Fabrica.Models.Patch.Builder;
using JetBrains.Annotations;

namespace Fabrica.Models.Support
{


    public interface IModel
    {


        long Id { get; }

        string Uid { get; set; }




    }


}

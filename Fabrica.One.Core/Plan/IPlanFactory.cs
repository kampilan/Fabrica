using System.IO;

namespace Fabrica.One.Plan
{


    public interface IPlanFactory
    {

        IPlan Create( Stream source );

    }


}

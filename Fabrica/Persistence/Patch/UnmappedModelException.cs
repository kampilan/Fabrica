using System;

namespace Fabrica.Persistence.Patch
{


    public class UnMappedModelException: Exception
    {

        public UnMappedModelException( string resource ): base( $"Could not find Entity using Name: ({resource})" )
        {
        }

    }


}

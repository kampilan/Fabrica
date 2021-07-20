using System.IO;
using System.Threading.Tasks;

namespace Fabrica.Press.Generation
{


    public interface ITemplateSourceProvider
    {

        Task<Stream> GetContent( string name );

    }


}

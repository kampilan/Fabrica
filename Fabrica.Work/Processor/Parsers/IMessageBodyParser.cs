using System.Threading.Tasks;

namespace Fabrica.Work.Processor.Parsers
{

    public interface IMessageBodyParser
    {

        Task<(bool ok, WorkRequest? request)> Parse( string body );


    }


}

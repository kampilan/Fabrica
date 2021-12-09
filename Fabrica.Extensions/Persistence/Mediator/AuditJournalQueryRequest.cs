using System.IO;
using Fabrica.Mediator;
using MediatR;

namespace Fabrica.Persistence.Mediator
{

    public class AuditJournalQueryRequest: IRequest<Response<MemoryStream>>
    {

        public string Entity { get; set; } = "";

        public string EntityUid { get; set; } = "";

    }


}

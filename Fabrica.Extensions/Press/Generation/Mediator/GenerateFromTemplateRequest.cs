using System.Collections.Generic;
using System.IO;
using Fabrica.Mediator;
using Fabrica.Press.Generation.DataSources;
using MediatR;

namespace Fabrica.Press.Generation.Mediator
{


    public class GenerateFromTemplatesRequest: IRequest<Response<Stream>>
    {


        public static GenerateFromTemplatesRequest Create( IEnumerable<IMergeDataSource> sources, IEnumerable<Stream> templates, OutputFormat format )
        {

            var request = new GenerateFromTemplatesRequest
            {
                Sources   = new List<IMergeDataSource>(sources),
                Templates = new List<Stream>( templates ),
                Format    = format
            };

            return request;

        }


        private GenerateFromTemplatesRequest()
        {
        }

        public List<IMergeDataSource> Sources { get; private set; }

        public List<Stream> Templates { get; private set; }

        public OutputFormat Format { get; private set; }

    }


}

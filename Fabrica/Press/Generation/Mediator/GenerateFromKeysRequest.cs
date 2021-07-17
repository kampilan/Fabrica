using System.Collections.Generic;
using System.IO;
using Fabrica.Mediator;
using Fabrica.Press.Generation.DataSources;
using MediatR;

namespace Fabrica.Press.Generation.Mediator
{

    public class GenerateFromKeysRequest: IRequest<Response<Stream>>
    {


        public static GenerateFromKeysRequest Create( IEnumerable<IMergeDataSource> sources, IEnumerable<string> keys, OutputFormat format )
        {

            var request = new GenerateFromKeysRequest
            {
                Sources      = new List<IMergeDataSource>(sources),
                TemplateKeys = new List<string>(keys),
                Format       = format
            };

            return request;

        }


        private GenerateFromKeysRequest()
        {
        }

        public List<IMergeDataSource> Sources { get; private set; }

        public List<string> TemplateKeys { get; private set; }

        public OutputFormat Format { get; private set; }

    }


}

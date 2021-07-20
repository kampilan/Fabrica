using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Press.Generation.Formatters;
using Fabrica.Press.Utilities;
using GemBox.Document;

namespace Fabrica.Press.Generation.Mediator
{

    public class GenerateFromTemplatesHandler: AbstractRequestHandler<GenerateFromTemplatesRequest,Stream>
    {



        public GenerateFromTemplatesHandler( ITemplateSourceProvider provider, IReadOnlyDictionary<string, IMergeFieldFormatter> formatters )
        {

            Provider = provider;
            Adapter = new MailMergeAdapter(formatters);

        }

        private ITemplateSourceProvider Provider { get; }
        private MailMergeAdapter Adapter { get; }


        protected override async Task<Stream> Perform(CancellationToken cancellationToken = default)
        {


            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to merge documents");
            var batch = new PdfBatch();
            foreach( var strm in Request.Templates )
            {

                await using ( strm )
                {

                    var document = DocumentModel.Load( strm, LoadOptions.DocxDefault );

                    var output = _merge(document);
                    batch.Add(output);

                }


            }



            var pdf = new MemoryStream();
            batch.Save(pdf);

            pdf.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            return pdf;


        }



        private Stream _merge(DocumentModel document)
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to set range prefixes");
            document.MailMerge.RangeStartPrefix = Adapter.RegionEnterPrefix;
            document.MailMerge.RangeEndPrefix = Adapter.RegionExitPrefix;



            // *****************************************************************
            logger.Debug("Attempting to dig out range names");
            var ranges = document.MailMerge.GetMergeFieldNames()
                .Where(r => r.StartsWith(Adapter.RegionEnterPrefix))
                .Select(r => r.Split(':').Last()).ToList();

            logger.Inspect("Range Count", ranges.Count);


            Adapter.Logger = logger;

            document.MailMerge.FieldMerging += (_, e) => Adapter.WhenFieldMerging(e);



            // *****************************************************************
            if (ranges.Count > 0)
            {

                logger.Debug("Attempting to merge each range");
                foreach (var ds in Request.Sources.Where(s => ranges.Contains(s.Region)))
                {
                    Adapter.Source = ds;
                    document.MailMerge.Execute(Adapter, ds.Region);
                }

            }
            else
            {

                logger.Debug("Attempting to merge global");
                Adapter.Global = Request.Sources.First(s => s.Region == "Root");
                document.MailMerge.Execute(Adapter);

            }



            // *****************************************************************
            logger.Debug("Attempting to save merged output to PDF");
            var output = new MemoryStream();
            document.Save(output, SaveOptions.PdfDefault);

            output.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            return output;


        }




    }


}

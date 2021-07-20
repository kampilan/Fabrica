using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fabrica.Press.Generation.DataSources;
using Fabrica.Press.Generation.Formatters;
using Fabrica.Press.Utilities;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Storage;
using GemBox.Document;

namespace Fabrica.Press.Generation
{


    public class GenerationComponent : CorrelatedObject
    {


        public GenerationComponent( ICorrelation correlation, IStorageProvider provider, string root, string path, IReadOnlyDictionary<string, IMergeFieldFormatter> formatters ) : base(correlation)
        {

            Provider = provider;
            Root     = root;
            Path     = path;

            Adapter = new MailMergeAdapter(formatters);

        }

        private IStorageProvider Provider { get; }
        private string Root { get; }
        private string Path { get; }

        private MailMergeAdapter Adapter { get; }



        public DocumentModel LoadDocx( Stream input )
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to load DocumentModel from DOCX stream");
            var document = DocumentModel.Load(input, LoadOptions.DocxDefault);



            // *****************************************************************
            return document;


        }

        public DocumentModel LoadHtml( Stream input )
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to load HTML");
            var document = DocumentModel.Load(input, LoadOptions.HtmlDefault);



            // *****************************************************************
            return document;


        }

        public DocumentModel LoadHtml( string html )
        {

            using var logger = EnterMethod();


            using (var strm = new MemoryStream())
            using (var writer = new StreamWriter(strm))
            {


                // *****************************************************************
                logger.Debug("Attempting to create stream from HTML string");
                writer.Write(html);
                writer.Flush();

                strm.Seek(0, SeekOrigin.Begin);



                // *****************************************************************
                logger.Debug("Attempting to load HTML");
                var document = DocumentModel.Load(strm, LoadOptions.HtmlDefault);



                // *****************************************************************
                return document;


            }


        }


        public async Task<DocumentModel> Combine( IList<string> keys )
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to build components");
            var components = new List<DocumentModel>();
            foreach (var key in keys)
            {

                var fullKey = $"{Path}/{key}";

                await using (var content = new MemoryStream())
                {

                    await Provider.GetAsync(Root, fullKey, content);

                    var component = DocumentModel.Load(content, LoadOptions.DocxDefault);
                    components.Add(component);

                }


            }



            // *****************************************************************
            logger.Debug("Attempting to combine components");
            var document = Combine(components);



            // *****************************************************************
            return document;


        }


        public DocumentModel Combine( IList<DocumentModel> components )
        {

            using var logger = EnterMethod();


            var document = new DocumentModel();
            var section = new Section(document);
            document.Sections.Add(section);


            foreach (var component in components)
            {

                var mapping = new ImportMapping(component, document, false);

                foreach (var b in component.Sections.SelectMany(s => s.Blocks))
                {
                    var ns = document.Import(b, true, mapping);
                    section.Blocks.Add(ns);
                }

            }


            return document;


        }


        public void Merge(DocumentModel document, IEnumerable<IMergeDataSource> sources)
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
                foreach (var ds in sources.Where(s => ranges.Contains(s.Region)))
                {
                    Adapter.Source = ds;
                    document.MailMerge.Execute(Adapter, ds.Region);
                }

            }
            else
            {

                logger.Debug("Attempting to merge global");
                Adapter.Global = sources.First(s => s.Region == "Root");
                document.MailMerge.Execute(Adapter);

            }


        }


        public void Merge(DocumentModel document, IEnumerable<IMergeDataSource> sources, PdfBatch batch)
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to perform merge");
            Merge(document, sources);



            // *****************************************************************
            using (var strm = new MemoryStream())
            {

                logger.Debug("Attempting to save document to PDF");
                document.Save(strm, SaveOptions.PdfDefault);


                logger.Debug("Attempting to add PDF to Batch");
                strm.Seek(0, SeekOrigin.Begin);
                batch.Add(strm);

            }


        }


        public void GeneratePdf(DocumentModel document, Stream output, bool rewind = true, bool autoClose = false)
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to Save Document to stream as PDF");
            document.Save(output, SaveOptions.PdfDefault);

            if (autoClose)
                output.Close();
            else if (rewind && output.CanSeek)
                output.Seek(0, SeekOrigin.Begin);


        }

        public void GenerateHtml(DocumentModel document, Stream output, bool rewind = true, bool autoClose = false)
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to Save Document to stream as PDF");
            document.Save(output, SaveOptions.HtmlDefault);

            if (autoClose)
                output.Close();
            else if (rewind && output.CanSeek)
                output.Seek(0, SeekOrigin.Begin);


        }

        public string GenerateHtml(DocumentModel document)
        {

            using var logger = EnterMethod();


            string html;
            using (var output = new MemoryStream())
            {

                // *****************************************************************
                logger.Debug("Attempting to Save Document to stream as PDF");
                document.Save(output, SaveOptions.HtmlDefault);

                output.Seek(0, SeekOrigin.Begin);



                // *****************************************************************
                logger.Debug("Attempting to reader HTML string from stream");
                using (var reader = new StreamReader(output))
                    html = reader.ReadToEnd();

            }



            // *****************************************************************
            return html;


        }


    }


}

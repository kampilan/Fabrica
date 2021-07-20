using System.IO;
using System.Threading.Tasks;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Storage;

namespace Fabrica.Press.Generation
{

    
    public class TemplateSourceProvider: CorrelatedObject, ITemplateSourceProvider
    {


        public TemplateSourceProvider( ICorrelation correlation, IStorageProvider provider, string root, string basePath): base(correlation)
        {

            Provider = provider;
            Root     = root;
            BasePath = basePath;

        }

        private IStorageProvider Provider { get; }
        private string Root { get; }
        private string BasePath { get; } 
        
        
        public async Task<Stream> GetContent( string name )
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(name), name);


            var fullName = $"{BasePath}/{name}";
            logger.Inspect(nameof(fullName), fullName);


            var content = new MemoryStream();
            await Provider.GetAsync(Root, fullName, content);


            return content;


        }


    }


}

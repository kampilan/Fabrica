using System.Drawing;
using System.IO;
using Fabrica.Press.Generation.Transformers;
using Fabrica.Press.Utilities;

namespace Fabrica.Press.Generation.Formatters
{
    
    public class SignatureFormatter: SignatureGenerator, IMergeFieldFormatter
    {

        public SignatureFormatter( SizeType size) : base(size)
        {
        }


        protected override object BuildResponse( Bitmap signature )
        {

            var ir =  (ImageResponse)base.BuildResponse(signature);

            var pt = new PictureTransformer
            {
                Format = PictureTransformer.FormatType.Png,
                Width  = ir.Width,
                Height = ir.Height
            };

            ir.Content.CopyTo(pt.Content);
            pt.Content.Seek(0, SeekOrigin.Begin);

            return pt;

        }


    }

}

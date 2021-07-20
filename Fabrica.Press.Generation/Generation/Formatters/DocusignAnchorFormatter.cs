using Fabrica.Press.Generation.Transformers;
using GemBox.Document;

namespace Fabrica.Press.Generation.Formatters
{


    public class DocusignAnchorFormatter: TemplateFormatter
    {


        public DocusignAnchorFormatter( string tag, string template ) : base(tag, template)
        {
        }


        public Color FontColor { get; set; } = Color.White;


        protected override object Format( object[] segments )
        {

            var value   =  base.Format(segments);
            var handler = new ColorRunTransformer( value.ToString(), FontColor );

            return handler;

        }


    }


}

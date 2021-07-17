using System.Linq;

namespace Fabrica.Press.Generation.Formatters
{

    
    public class TemplateFormatter: IMergeFieldFormatter
    {


        public TemplateFormatter( string tag, string template )
        {
            Tag      = tag;
            Template = template;
        }


        public string Template { get; }


        protected virtual object[] Parse( object value )
        {
            var segs = value is string ? value.ToString().Split('|').Cast<object>().ToArray() : new[] { value };
            return segs;
        }

        protected virtual object Format( object[] segments )
        {
            var formatted = string.Format( Template, segments );
            return formatted;
        }

        
        #region IMergeFieldFormatter

        public string Tag { get; }

        public object Render( object value )
        {

            var segs      = Parse(value);
            var formatted = Format(segs);

            return formatted;

        }

        #endregion


    }


}

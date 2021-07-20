using GemBox.Document;

namespace Fabrica.Press.Generation.Transformers
{


    public class ColorRunTransformer: IInlineTransformer
    {


        public ColorRunTransformer( string value, Color color )
        {

            Value = value;
            Color = color;

        }

        private string Value { get; }
        private Color Color { get; }


        public Inline Handle( Inline target )
        {

            if (target is Run run)
            {
                run.CharacterFormat.FontColor = Color;
                run.Text = Value;
            }

            return target;

        }

    }


}

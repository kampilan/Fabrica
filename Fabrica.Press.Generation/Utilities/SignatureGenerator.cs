using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using Fabrica.Press.Generation.Formatters;
using Fabrica.Watch;
using JetBrains.Annotations;
using Color = System.Drawing.Color;

namespace Fabrica.Press.Utilities
{


    public class SignatureGenerator
    {

        public enum SizeType { Sig50, Sig60, Sig70, Sig80, Sig90, Sig }


        protected class Specification
        {

            public string Signer { get; set; } = "";

            public string Topline { get; set; } = "";
            public string Bottomline { get; set; } = "";

            public bool DrawBox { get; set; } = true;


        }

        protected class Geometery
        {

            public Geometery( int width, int height )
            {

                Width  = width;
                Height = height;

                MaxSigFontSize = Convert.ToInt32(Math.Floor(Convert.ToDouble(height) * .82));
                
                var labelHeight = Convert.ToInt32(Math.Floor(Convert.ToDouble(height) *.14));
                LabelFontSize = labelHeight - 2;

                Box        = new Rectangle( 3, 3, Width-7, Height-7);
                Topline    = new Rectangle( 160, 1, Width-292, LabelFontSize+10 );
                Signature  = new Rectangle( 8, 8, Width-12, Height-12 );
                Bottomline = new Rectangle( 160, Height-LabelFontSize-12, Width-292, LabelFontSize+10 );

            }

            public int Width { get; }
            public int Height { get; }

            public Rectangle Box { get; set; }

            public int MaxSigFontSize { get; set; }
            public int LabelFontSize { get; set; }

            public Rectangle Topline { get; set; }

            public Rectangle Signature { get; set; }

            public Rectangle Bottomline { get; set; }

        }


        public SignatureGenerator( SizeType size )
        {

            Tag = size.ToString().ToUpper();

            switch( size )
            {
                case SizeType.Sig50:
                    Width  = 100;
                    Height = 25;
                    break;
                case SizeType.Sig60:
                    Width  = 120;
                    Height = 30;
                    break;
                case SizeType.Sig70:
                    Width  = 140;
                    Height = 35;
                    break;
                case SizeType.Sig80:
                    Width  = 160;
                    Height = 40;
                    break;
                case SizeType.Sig90:
                    Width  = 180;
                    Height = 45;
                    break;
                case SizeType.Sig:
                    Width  = 200;
                    Height = 50;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }

        }

        public int Width { get; }
        public int Height { get; }

        public Color BackgroundColor { get; set; } = Color.White;
        public Color BoxColor  { get; set; } = Color.CornflowerBlue;
        public Color SignatureColor { get; set; } = Color.Black;
        public Color LabelColor { get; set; } = Color.Black;


        public string SigFontName    { get; set; } = "Freestyle Script LET";
        public string LabelFontName { get; set; } = "Arial";


        public TextRenderingHint Hint { get; set; } = TextRenderingHint.AntiAliasGridFit;



        protected virtual Specification Parse([NotNull] string signatureTag)
        {

            if (signatureTag == null) throw new ArgumentNullException(nameof(signatureTag));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(signatureTag), signatureTag);


                var spec = new Specification();



                // *****************************************************************
                logger.Debug("Attempting to parse signatureTag");
                var segs = signatureTag.Split('|');

                logger.Inspect(nameof(segs.Length), segs.Length);



                // *****************************************************************
                logger.Debug("Attempting to dig out Specification parts");
                if (segs.Length == 1)
                {
                    spec.Signer = segs[0];
                    spec.Bottomline = segs[0];
                }
                else if (segs.Length == 2)
                {
                    spec.Signer = segs[0];
                    spec.Bottomline = segs[1];
                }
                else if (segs.Length == 3)
                {
                    spec.Signer = segs[0];
                    spec.Topline = segs[1];
                    spec.Bottomline = segs[2];
                }
                else if (segs.Length == 4)
                {
                    spec.Signer = segs[0];
                    spec.Topline = segs[1];
                    spec.Bottomline = segs[2];
                    spec.DrawBox = segs[3] != "0";
                }

                logger.LogObject(nameof(spec), spec);



                // *****************************************************************
                return spec;


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual Geometery BuildGeometery()
        {
            var geo = new Geometery(800, 200);
            return geo;
        }

        protected virtual Bitmap Generate( [NotNull] Specification spec, [NotNull] Geometery geo )
        {

            if (spec == null) throw new ArgumentNullException(nameof(spec));
            if (geo == null) throw new ArgumentNullException(nameof(geo));


            // *****************************************************************************
            var signatureImage = new Bitmap( geo.Width, geo.Height);
            signatureImage.MakeTransparent();
            using (var signatureGraphic = Graphics.FromImage(signatureImage))
            {
                signatureGraphic.Clear(BackgroundColor);
            }



            // *****************************************************************************
            var installedFontCollection = new InstalledFontCollection();

            Font sigFont = null;
            if (installedFontCollection.Families.Any(f => f.Name == SigFontName))
                sigFont = new Font(SigFontName, geo.MaxSigFontSize);

            if (sigFont == null)
                throw new Exception($"Signature Font: {SigFontName} not found");

            Font labelFont = null;
            if (installedFontCollection.Families.Any(f => f.Name == LabelFontName))
                labelFont = new Font(LabelFontName, geo.LabelFontSize);

            if (labelFont == null)
                throw new Exception($"Label Font: {LabelFontName} not found");



            // *****************************************************************************
            var curSigFontSize = geo.MaxSigFontSize;

            using (var signatureGraphic = Graphics.FromImage(signatureImage))
            {

                signatureGraphic.TextRenderingHint = Hint;

                while (true)
                {
                    var size = signatureGraphic.MeasureString(spec.Signer, sigFont);
                    if (size.Width > geo.Width-13 || size.Height > geo.Height-13)
                    {
                        curSigFontSize -= 1;
                        sigFont.Dispose();
                        sigFont = new Font(SigFontName, curSigFontSize);
                    }
                    else
                        break;
                }

                var labelFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                var sigFormat   = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                if (spec.DrawBox)
                {
                    var boxRect = geo.Box;
                    var boxPen = new Pen(BoxColor, 4);
                    signatureGraphic.DrawRectangle(boxPen, boxRect);
                }

                if( !string.IsNullOrWhiteSpace(spec.Topline) )
                {
                    var rectTop = geo.Topline;
                    signatureGraphic.FillRectangle(new SolidBrush(BackgroundColor), rectTop);
                    signatureGraphic.DrawString(spec.Topline, labelFont, new SolidBrush(LabelColor), rectTop, labelFormat);
                }

                if( !string.IsNullOrWhiteSpace(spec.Bottomline) )
                {
                    var rectBot = geo.Bottomline;
                    signatureGraphic.FillRectangle(new SolidBrush(BackgroundColor), rectBot);
                    signatureGraphic.DrawString(spec.Bottomline, labelFont, new SolidBrush(LabelColor), rectBot, labelFormat);
                }

                var sigRect = geo.Signature;
                signatureGraphic.DrawString(spec.Signer, sigFont, new SolidBrush(SignatureColor), sigRect, sigFormat);

            }


            return signatureImage;

        }


        protected virtual Bitmap Resize( Bitmap source )
        {

            var destRect  = new Rectangle(0, 0, Width, Height);
            var destImage = new Bitmap(Width, Height);

            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode    = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode      = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;

        }

        protected virtual object BuildResponse( [NotNull] Bitmap signature )
        {

            if (signature == null) throw new ArgumentNullException(nameof(signature));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var output = Resize(signature);


                // *****************************************************************
                logger.Debug("Attempting to build ImageResponse");
                var response = new ImageResponse
                {
                    Format = ImageResponse.FormatType.Png,
                    Height = Height,
                    Width  = Width
                };


                output.Save(response.Content, ImageFormat.Png);
                response.Content.Seek(0, SeekOrigin.Begin);


                // *****************************************************************
                return response;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public string Tag { get; protected set; }


        public object Render( object value )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                var geo = BuildGeometery();


                // *****************************************************************
                if( value == null )
                {
                    logger.Debug("Null Value submitted generating ? signature");
                    var blank    = Generate( new Specification {Signer = "??????????", DrawBox = true}, geo );
                    return BuildResponse( blank );
                }



                // *****************************************************************
                logger.Debug("Attempting to convert value to string");
                var signatureTag = value.ToString();

                logger.Inspect( nameof(signatureTag), signatureTag );



                // *****************************************************************
                logger.Debug("Attempting to parse Signature Tag");
                var spec = Parse( signatureTag );

                logger.LogObject( nameof(spec), spec );



                // *****************************************************************
                logger.Debug( "Attempting to create signature Bitmap from Specification" );
                var bm = Generate( spec, geo );



                // *****************************************************************
                logger.Debug("Attempting to build Response");
                var response = BuildResponse( bm );



                // *****************************************************************
                return response;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}

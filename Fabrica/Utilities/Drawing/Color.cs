//
// System.Drawing.Color.cs
//
// Authors:
// 	Dennis Hayes (dennish@raytek.com)
// 	Ben Houston  (ben@exocortex.org)
// 	Gonzalo Paniagua (gonzalo@ximian.com)
// 	Juraj Skripsky (juraj@hotfeet.ch)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Dennis Hayes
// (c) 2002 Ximian, Inc. (http://www.ximiam.com)
// (C) 2005 HotFeet GmbH (http://www.hotfeet.ch)
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Fabrica.Utilities.Drawing
{

    public struct Color
    {

        // Private transparency (A) and R,G,B fields.
        private long _value;

        // The specs also indicate that all three of these properties are true
        // if created with FromKnownColor or FromNamedColor, false otherwise (FromARGB).
        // Per Microsoft and ECMA specs these varibles are set by which constructor is used, not by their values.
        [Flags]
        internal enum ColorType : short
        {
            Empty = 0,
            Known = 1,
            ARGB = 2,
            Named = 4,
            System = 8
        }

        internal short state;
        internal short knownColor;
        // #if ONLY_1_1
        // Mono bug #324144 is holding this change
        // MS 1.1 requires this member to be present for serialization (not so in 2.0)
        // however it's bad to keep a string (reference) in a struct
        internal string name;
        // #endif
#if TARGET_JVM
    internal java.awt.Color NativeObject {
      get {
        return new java.awt.Color (R, G, B, A);
      }
    }

    internal static Color FromArgbNamed (int alpha, int red, int green, int blue, string name, KnownColor knownColor)
    {
      Color color = FromArgb (alpha, red, green, blue);
      color.state = (short) (ColorType.Known|ColorType.Named);
      color.name = KnownColors.GetName (knownColor);
      color.knownColor = (short) knownColor;
      return color;
    }

    internal static Color FromArgbSystem (int alpha, int red, int green, int blue, string name, KnownColor knownColor)
    {
      Color color = FromArgbNamed (alpha, red, green, blue, name, knownColor);
      color.state |= (short) ColorType.System;
      return color;
    }
#endif

        public string Name
        {
            get
            {
#if NET_2_0_ONCE_MONO_BUG_324144_IS_FIXED
        if (IsNamedColor)
          return KnownColors.GetName (knownColor);
        else
          return String.Format ("{0:x}", ToArgb ());
#else
                // name is required for serialization under 1.x, but not under 2.0
                return name ?? (name = IsNamedColor ? KnownColors.GetName(knownColor) : $"{ToArgb():x}");
#endif
            }
        }

        public bool IsKnownColor => (state & ((short)ColorType.Known)) != 0;

        public bool IsSystemColor => (state & ((short)ColorType.System)) != 0;

        public bool IsNamedColor => (state & (short)(ColorType.Known | ColorType.Named)) != 0;

        internal long Value
        {
            get
            {
                // Optimization for known colors that were deserialized
                // from an MS serialized stream.  
                if (_value == 0 && IsKnownColor)
                {
                    _value = KnownColors.FromKnownColor((KnownColor)knownColor).ToArgb() & 0xFFFFFFFF;
                }
                return _value;
            }
            set => _value = value;
        }

        public static Color FromArgb(int red, int green, int blue)
        {
            return FromArgb(255, red, green, blue);
        }

        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            CheckArgbValues(alpha, red, green, blue);
            var color = new Color
            {
                state = (short) ColorType.ARGB,
                Value = (int) ((uint) alpha << 24) + (red << 16) + (green << 8) + blue
            };
            return color;
        }

        public int ToArgb()
        {
            return (int)Value;
        }

        public static Color FromArgb(int alpha, Color baseColor)
        {
            return FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
        }

        public static Color FromArgb(int argb)
        {
            return FromArgb((argb >> 24) & 0x0FF, (argb >> 16) & 0x0FF, (argb >> 8) & 0x0FF, argb & 0x0FF);
        }

        public static Color FromKnownColor(KnownColor color)
        {
            return KnownColors.FromKnownColor(color);
        }

        public static Color FromName(string name)
        {
            try
            {
                KnownColor kc = (KnownColor)Enum.Parse(typeof(KnownColor), name, true);
                return KnownColors.FromKnownColor(kc);
            }
            catch
            {
                // This is what it returns! 	 
                Color d = FromArgb(0, 0, 0, 0);
                d.name = name;
                d.state |= (short)ColorType.Named;
                return d;
            }
        }

        // -----------------------
        // Public Shared Members
        // -----------------------

        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized Color Structure
        /// </remarks>

        // ReSharper disable once UnassignedReadonlyField
        public static readonly Color Empty;

        /// <summary>
        ///	Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two Color objects. The return value is
        ///	based on the equivalence of the A,R,G,B properties 
        ///	of the two Colors.
        /// </remarks>

        public static bool operator ==(Color left, Color right)
        {
            if (left.Value != right.Value)
                return false;
            if (left.IsNamedColor != right.IsNamedColor)
                return false;
            if (left.IsSystemColor != right.IsSystemColor)
                return false;
            if (left.IsEmpty != right.IsEmpty)
                return false;
            if (left.IsNamedColor)
            {
                // then both are named (see previous check) and so we need to compare them
                // but otherwise we don't as it kills performance (Name calls String.Format)
                if (left.Name != right.Name)
                    return false;
            }
            return true;
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two Color objects. The return value is
        ///	based on the equivalence of the A,R,G,B properties 
        ///	of the two colors.
        /// </remarks>

        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        public float GetBrightness()
        {
            byte minval = Math.Min(R, Math.Min(G, B));
            byte maxval = Math.Max(R, Math.Max(G, B));

            return (float)(maxval + minval) / 510;
        }

        public float GetSaturation()
        {
            byte minval = Math.Min(R, Math.Min(G, B));
            byte maxval = Math.Max(R, Math.Max(G, B));

            if (maxval == minval)
                return 0.0f;

            int sum = maxval + minval;
            if (sum > 255)
                sum = 510 - sum;

            return (float)(maxval - minval) / sum;
        }

        public float GetHue()
        {
            int r = R;
            int g = G;
            int b = B;
            byte minval = (byte)Math.Min(r, Math.Min(g, b));
            byte maxval = (byte)Math.Max(r, Math.Max(g, b));

            if (maxval == minval)
                return 0.0f;

            float diff = maxval - minval;
            float rnorm = (maxval - r) / diff;
            float gnorm = (maxval - g) / diff;
            float bnorm = (maxval - b) / diff;

            float hue = 0.0f;
            if (r == maxval)
                hue = 60.0f * (6.0f + bnorm - gnorm);
            if (g == maxval)
                hue = 60.0f * (2.0f + rnorm - bnorm);
            if (b == maxval)
                hue = 60.0f * (4.0f + gnorm - rnorm);
            if (hue > 360.0f)
                hue = hue - 360.0f;

            return hue;
        }

        // -----------------------
        // Public Instance Members
        // -----------------------

        /// <summary>
        ///	ToKnownColor method
        /// </summary>
        ///
        /// <remarks>
        ///	Returns the KnownColor enum value for this color, 0 if is not known.
        /// </remarks>
        public KnownColor ToKnownColor()
        {
            return (KnownColor)knownColor;
        }

        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates transparent black. R,G,B = 0; A=0?
        /// </remarks>

        public bool IsEmpty => state == (short)ColorType.Empty;

        public byte A => (byte)(Value >> 24);

        public byte R => (byte)(Value >> 16);

        public byte G => (byte)(Value >> 8);

        public byte B => (byte)Value;

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this Color and another object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is Color))
                return false;
            Color c = (Color)obj;
            return this == c;
        }

        /// <summary>
        ///	Reference Equals Method
        ///	Is commented out because this is handled by the base class.
        ///	TODO: Is it correct to let the base class handel reference equals
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this Color and another object.
        /// </remarks>
        //public bool ReferenceEquals (object o)
        //{
        //	if (!(o is Color))return false;
        //	return (this == (Color) o);
        //}



        /// <summary>
        ///	GetHashCode Method
        /// </summary>
        ///
        /// <remarks>
        ///	Calculates a hashing value.
        /// </remarks>

        public override int GetHashCode()
        {
            int hc = (int)(Value ^ (Value >> 32) ^ state ^ (knownColor >> 16));
            if (IsNamedColor)
                hc ^= Name.GetHashCode();
            return hc;
        }

        /// <summary>
        ///	ToString Method
        /// </summary>
        ///
        /// <remarks>
        ///	Formats the Color as a string in ARGB notation.
        /// </remarks>

        [NotNull]
        public override string ToString()
        {
            if (IsEmpty)
                return "Color [Empty]";

            // Use the property here, not the field.
            if (IsNamedColor)
                return "Color [" + Name + "]";

            return $"Color [A={A}, R={R}, G={G}, B={B}]";
        }

        private static void CheckRgbValues(int red, int green, int blue)
        {
            if ((red > 255) || (red < 0))
                throw CreateColorArgumentException(red, "red");
            if ((green > 255) || (green < 0))
                throw CreateColorArgumentException(green, "green");
            if ((blue > 255) || (blue < 0))
                throw CreateColorArgumentException(blue, "blue");
        }

        [NotNull]
        private static ArgumentException CreateColorArgumentException(int value, string color)
        {
            return new ArgumentException(string.Format("'{0}' is not a valid"
              + " value for '{1}'. '{1}' should be greater or equal to 0 and"
              + " less than or equal to 255.", value, color));
        }

        private static void CheckArgbValues(int alpha, int red, int green, int blue)
        {
            if ((alpha > 255) || (alpha < 0))
                throw CreateColorArgumentException(alpha, "alpha");
            CheckRgbValues(red, green, blue);
        }


        public static Color Transparent => KnownColors.FromKnownColor(KnownColor.Transparent);

        public static Color AliceBlue => KnownColors.FromKnownColor(KnownColor.AliceBlue);

        public static Color AntiqueWhite => KnownColors.FromKnownColor(KnownColor.AntiqueWhite);

        public static Color Aqua => KnownColors.FromKnownColor(KnownColor.Aqua);

        public static Color Aquamarine => KnownColors.FromKnownColor(KnownColor.Aquamarine);

        public static Color Azure => KnownColors.FromKnownColor(KnownColor.Azure);

        public static Color Beige => KnownColors.FromKnownColor(KnownColor.Beige);

        public static Color Bisque => KnownColors.FromKnownColor(KnownColor.Bisque);

        public static Color Black => KnownColors.FromKnownColor(KnownColor.Black);

        public static Color BlanchedAlmond => KnownColors.FromKnownColor(KnownColor.BlanchedAlmond);

        public static Color Blue => KnownColors.FromKnownColor(KnownColor.Blue);

        public static Color BlueViolet => KnownColors.FromKnownColor(KnownColor.BlueViolet);

        public static Color Brown => KnownColors.FromKnownColor(KnownColor.Brown);

        public static Color BurlyWood => KnownColors.FromKnownColor(KnownColor.BurlyWood);

        public static Color CadetBlue => KnownColors.FromKnownColor(KnownColor.CadetBlue);

        public static Color Chartreuse => KnownColors.FromKnownColor(KnownColor.Chartreuse);

        public static Color Chocolate => KnownColors.FromKnownColor(KnownColor.Chocolate);

        public static Color Coral => KnownColors.FromKnownColor(KnownColor.Coral);

        public static Color CornflowerBlue => KnownColors.FromKnownColor(KnownColor.CornflowerBlue);

        public static Color Cornsilk => KnownColors.FromKnownColor(KnownColor.Cornsilk);

        public static Color Crimson => KnownColors.FromKnownColor(KnownColor.Crimson);

        public static Color Cyan => KnownColors.FromKnownColor(KnownColor.Cyan);

        public static Color DarkBlue => KnownColors.FromKnownColor(KnownColor.DarkBlue);

        public static Color DarkCyan => KnownColors.FromKnownColor(KnownColor.DarkCyan);

        public static Color DarkGoldenrod => KnownColors.FromKnownColor(KnownColor.DarkGoldenrod);

        public static Color DarkGray => KnownColors.FromKnownColor(KnownColor.DarkGray);

        public static Color DarkGreen => KnownColors.FromKnownColor(KnownColor.DarkGreen);

        public static Color DarkKhaki => KnownColors.FromKnownColor(KnownColor.DarkKhaki);

        public static Color DarkMagenta => KnownColors.FromKnownColor(KnownColor.DarkMagenta);

        public static Color DarkOliveGreen => KnownColors.FromKnownColor(KnownColor.DarkOliveGreen);

        public static Color DarkOrange => KnownColors.FromKnownColor(KnownColor.DarkOrange);

        public static Color DarkOrchid => KnownColors.FromKnownColor(KnownColor.DarkOrchid);

        public static Color DarkRed => KnownColors.FromKnownColor(KnownColor.DarkRed);

        public static Color DarkSalmon => KnownColors.FromKnownColor(KnownColor.DarkSalmon);

        public static Color DarkSeaGreen => KnownColors.FromKnownColor(KnownColor.DarkSeaGreen);

        public static Color DarkSlateBlue => KnownColors.FromKnownColor(KnownColor.DarkSlateBlue);

        public static Color DarkSlateGray => KnownColors.FromKnownColor(KnownColor.DarkSlateGray);

        public static Color DarkTurquoise => KnownColors.FromKnownColor(KnownColor.DarkTurquoise);

        public static Color DarkViolet => KnownColors.FromKnownColor(KnownColor.DarkViolet);

        public static Color DeepPink => KnownColors.FromKnownColor(KnownColor.DeepPink);

        public static Color DeepSkyBlue => KnownColors.FromKnownColor(KnownColor.DeepSkyBlue);

        public static Color DimGray => KnownColors.FromKnownColor(KnownColor.DimGray);

        public static Color DodgerBlue => KnownColors.FromKnownColor(KnownColor.DodgerBlue);

        public static Color Firebrick => KnownColors.FromKnownColor(KnownColor.Firebrick);

        public static Color FloralWhite => KnownColors.FromKnownColor(KnownColor.FloralWhite);

        public static Color ForestGreen => KnownColors.FromKnownColor(KnownColor.ForestGreen);

        public static Color Fuchsia => KnownColors.FromKnownColor(KnownColor.Fuchsia);

        public static Color Gainsboro => KnownColors.FromKnownColor(KnownColor.Gainsboro);

        public static Color GhostWhite => KnownColors.FromKnownColor(KnownColor.GhostWhite);

        public static Color Gold => KnownColors.FromKnownColor(KnownColor.Gold);

        public static Color Goldenrod => KnownColors.FromKnownColor(KnownColor.Goldenrod);

        public static Color Gray => KnownColors.FromKnownColor(KnownColor.Gray);

        public static Color Green => KnownColors.FromKnownColor(KnownColor.Green);

        public static Color GreenYellow => KnownColors.FromKnownColor(KnownColor.GreenYellow);

        public static Color Honeydew => KnownColors.FromKnownColor(KnownColor.Honeydew);

        public static Color HotPink => KnownColors.FromKnownColor(KnownColor.HotPink);

        public static Color IndianRed => KnownColors.FromKnownColor(KnownColor.IndianRed);

        public static Color Indigo => KnownColors.FromKnownColor(KnownColor.Indigo);

        public static Color Ivory => KnownColors.FromKnownColor(KnownColor.Ivory);

        public static Color Khaki => KnownColors.FromKnownColor(KnownColor.Khaki);

        public static Color Lavender => KnownColors.FromKnownColor(KnownColor.Lavender);

        public static Color LavenderBlush => KnownColors.FromKnownColor(KnownColor.LavenderBlush);

        public static Color LawnGreen => KnownColors.FromKnownColor(KnownColor.LawnGreen);

        public static Color LemonChiffon => KnownColors.FromKnownColor(KnownColor.LemonChiffon);

        public static Color LightBlue => KnownColors.FromKnownColor(KnownColor.LightBlue);

        public static Color LightCoral => KnownColors.FromKnownColor(KnownColor.LightCoral);

        public static Color LightCyan => KnownColors.FromKnownColor(KnownColor.LightCyan);

        public static Color LightGoldenrodYellow => KnownColors.FromKnownColor(KnownColor.LightGoldenrodYellow);

        public static Color LightGreen => KnownColors.FromKnownColor(KnownColor.LightGreen);

        public static Color LightGray => KnownColors.FromKnownColor(KnownColor.LightGray);

        public static Color LightPink => KnownColors.FromKnownColor(KnownColor.LightPink);

        public static Color LightSalmon => KnownColors.FromKnownColor(KnownColor.LightSalmon);

        public static Color LightSeaGreen => KnownColors.FromKnownColor(KnownColor.LightSeaGreen);

        public static Color LightSkyBlue => KnownColors.FromKnownColor(KnownColor.LightSkyBlue);

        public static Color LightSlateGray => KnownColors.FromKnownColor(KnownColor.LightSlateGray);

        public static Color LightSteelBlue => KnownColors.FromKnownColor(KnownColor.LightSteelBlue);

        public static Color LightYellow => KnownColors.FromKnownColor(KnownColor.LightYellow);

        public static Color Lime => KnownColors.FromKnownColor(KnownColor.Lime);

        public static Color LimeGreen => KnownColors.FromKnownColor(KnownColor.LimeGreen);

        public static Color Linen => KnownColors.FromKnownColor(KnownColor.Linen);

        public static Color Magenta => KnownColors.FromKnownColor(KnownColor.Magenta);

        public static Color Maroon => KnownColors.FromKnownColor(KnownColor.Maroon);

        public static Color MediumAquamarine => KnownColors.FromKnownColor(KnownColor.MediumAquamarine);

        public static Color MediumBlue => KnownColors.FromKnownColor(KnownColor.MediumBlue);

        public static Color MediumOrchid => KnownColors.FromKnownColor(KnownColor.MediumOrchid);

        public static Color MediumPurple => KnownColors.FromKnownColor(KnownColor.MediumPurple);

        public static Color MediumSeaGreen => KnownColors.FromKnownColor(KnownColor.MediumSeaGreen);

        public static Color MediumSlateBlue => KnownColors.FromKnownColor(KnownColor.MediumSlateBlue);

        public static Color MediumSpringGreen => KnownColors.FromKnownColor(KnownColor.MediumSpringGreen);

        public static Color MediumTurquoise => KnownColors.FromKnownColor(KnownColor.MediumTurquoise);

        public static Color MediumVioletRed => KnownColors.FromKnownColor(KnownColor.MediumVioletRed);

        public static Color MidnightBlue => KnownColors.FromKnownColor(KnownColor.MidnightBlue);

        public static Color MintCream => KnownColors.FromKnownColor(KnownColor.MintCream);

        public static Color MistyRose => KnownColors.FromKnownColor(KnownColor.MistyRose);

        public static Color Moccasin => KnownColors.FromKnownColor(KnownColor.Moccasin);

        public static Color NavajoWhite => KnownColors.FromKnownColor(KnownColor.NavajoWhite);

        public static Color Navy => KnownColors.FromKnownColor(KnownColor.Navy);

        public static Color OldLace => KnownColors.FromKnownColor(KnownColor.OldLace);

        public static Color Olive => KnownColors.FromKnownColor(KnownColor.Olive);

        public static Color OliveDrab => KnownColors.FromKnownColor(KnownColor.OliveDrab);

        public static Color Orange => KnownColors.FromKnownColor(KnownColor.Orange);

        public static Color OrangeRed => KnownColors.FromKnownColor(KnownColor.OrangeRed);

        public static Color Orchid => KnownColors.FromKnownColor(KnownColor.Orchid);

        public static Color PaleGoldenrod => KnownColors.FromKnownColor(KnownColor.PaleGoldenrod);

        public static Color PaleGreen => KnownColors.FromKnownColor(KnownColor.PaleGreen);

        public static Color PaleTurquoise => KnownColors.FromKnownColor(KnownColor.PaleTurquoise);

        public static Color PaleVioletRed => KnownColors.FromKnownColor(KnownColor.PaleVioletRed);

        public static Color PapayaWhip => KnownColors.FromKnownColor(KnownColor.PapayaWhip);

        public static Color PeachPuff => KnownColors.FromKnownColor(KnownColor.PeachPuff);

        public static Color Peru => KnownColors.FromKnownColor(KnownColor.Peru);

        public static Color Pink => KnownColors.FromKnownColor(KnownColor.Pink);

        public static Color Plum => KnownColors.FromKnownColor(KnownColor.Plum);

        public static Color PowderBlue => KnownColors.FromKnownColor(KnownColor.PowderBlue);

        public static Color Purple => KnownColors.FromKnownColor(KnownColor.Purple);

        public static Color Red => KnownColors.FromKnownColor(KnownColor.Red);

        public static Color RosyBrown => KnownColors.FromKnownColor(KnownColor.RosyBrown);

        public static Color RoyalBlue => KnownColors.FromKnownColor(KnownColor.RoyalBlue);

        public static Color SaddleBrown => KnownColors.FromKnownColor(KnownColor.SaddleBrown);

        public static Color Salmon => KnownColors.FromKnownColor(KnownColor.Salmon);

        public static Color SandyBrown => KnownColors.FromKnownColor(KnownColor.SandyBrown);

        public static Color SeaGreen => KnownColors.FromKnownColor(KnownColor.SeaGreen);

        public static Color SeaShell => KnownColors.FromKnownColor(KnownColor.SeaShell);

        public static Color Sienna => KnownColors.FromKnownColor(KnownColor.Sienna);

        public static Color Silver => KnownColors.FromKnownColor(KnownColor.Silver);

        public static Color SkyBlue => KnownColors.FromKnownColor(KnownColor.SkyBlue);

        public static Color SlateBlue => KnownColors.FromKnownColor(KnownColor.SlateBlue);

        public static Color SlateGray => KnownColors.FromKnownColor(KnownColor.SlateGray);

        public static Color Snow => KnownColors.FromKnownColor(KnownColor.Snow);

        public static Color SpringGreen => KnownColors.FromKnownColor(KnownColor.SpringGreen);

        public static Color SteelBlue => KnownColors.FromKnownColor(KnownColor.SteelBlue);

        public static Color Tan => KnownColors.FromKnownColor(KnownColor.Tan);

        public static Color Teal => KnownColors.FromKnownColor(KnownColor.Teal);

        public static Color Thistle => KnownColors.FromKnownColor(KnownColor.Thistle);

        public static Color Tomato => KnownColors.FromKnownColor(KnownColor.Tomato);

        public static Color Turquoise => KnownColors.FromKnownColor(KnownColor.Turquoise);

        public static Color Violet => KnownColors.FromKnownColor(KnownColor.Violet);

        public static Color Wheat => KnownColors.FromKnownColor(KnownColor.Wheat);

        public static Color White => KnownColors.FromKnownColor(KnownColor.White);

        public static Color WhiteSmoke => KnownColors.FromKnownColor(KnownColor.WhiteSmoke);

        public static Color Yellow => KnownColors.FromKnownColor(KnownColor.Yellow);

        public static Color YellowGreen => KnownColors.FromKnownColor(KnownColor.YellowGreen);
    }
}
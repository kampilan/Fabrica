using System.Text;

namespace Fabrica.Utilities.Text;

public class Base62Converter
{

    public static string NewGuid()
    {

        var guid = Guid.NewGuid();

        var str = Encode( guid.ToByteArray() );

        return str;

    }

    public static string Encode( byte[] buf )
    {

        var converted = BaseConvert( buf, 256, 62 );
        var builder = new StringBuilder();
        foreach( var t in converted )
            builder.Append(CharacterSet[t]);

        return builder.ToString();

    }

    private const string CharacterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static IEnumerable<byte> BaseConvert(byte[] source, int sourceBase, int targetBase)
    {
        var result = new List<int>();
        int count;
        while ((count = source.Length) > 0)
        {
            var quotient = new List<byte>();
            int remainder = 0;
            for (var i = 0; i != count; i++)
            {
                int accumulator = source[i] + remainder * sourceBase;
                byte digit = Convert.ToByte((accumulator - (accumulator % targetBase)) / targetBase);
                remainder = accumulator % targetBase;
                if (quotient.Count > 0 || digit != 0)
                {
                    quotient.Add(digit);
                }
            }

            result.Insert(0, remainder);
            source = quotient.ToArray();
        }

        var output = new byte[result.Count];
        for (int i = 0; i < result.Count; i++)
            output[i] = (byte)result[i];

        return output;
    }

}
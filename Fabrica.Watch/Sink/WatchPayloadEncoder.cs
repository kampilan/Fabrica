using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;

namespace Fabrica.Watch.Sink;

public static class WatchPayloadEncoder
{

    private static bool IsSupported { get; } = Avx2.IsSupported;

    public static string Encode( string payload )
    {
        var buf = Encoding.ASCII.GetBytes(payload);
        return Encode(buf);
    }

    public static string Encode( byte[]? buf )
    {

        if( buf is null || buf.Length == 0 )
            return "";


        string base64;
        if( IsSupported )
        {

            var len = (1 + (buf.Length - 1) / 3) * 4;
            var outChar = new char[len];

            if( TryToBase64Chars( buf, outChar, out var count ) )
                base64 = new string(outChar[..count]);
            else
                base64 = Convert.ToBase64String(buf);

        }
        else
            base64 = Convert.ToBase64String(buf);

        return base64;

    }


    public static string DecodeToString(string base64)
    {
        var buf = Decode(base64);
        var str = Encoding.ASCII.GetString(buf);
        return str;
    }


    public static byte[] Decode( string base64 )
    {


        byte[] buf;
        if (IsSupported)
        {
            var chars = base64.ToCharArray();
            var len = (base64.Length * 3 + 3) / 4 - (base64.Length > 0 && base64[^1] == '=' ? base64.Length > 1 && base64[^2] == '=' ? 2 : 1 : 0);
            buf = new byte[len];

            if( !TryFromBase64Chars(chars, buf, out _) )
                buf = Convert.FromBase64CharArray(chars,0, chars.Length);

        }
        else
            buf = Convert.FromBase64String(base64);


        return buf;


    }



    private static unsafe bool TryToBase64Chars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten)
    {
        var inputLength = bytes.Length;
        charsWritten = 0;
        if (inputLength == 0)
        {
            return true;
        }

        var outputLength = chars.Length;
        var expectedLength = (1 + (inputLength - 1) / 3) * 4;
        if (outputLength < expectedLength)
        {
            return false;
        }

        var permuter = Vector256.Create(0, 0, 1, 2, 3, 4, 5, 6);
        var mask1 = Vector256.Create(0x0fc0fc00).AsByte();
        var shift1 = Vector256.Create(0x04000040).AsUInt16();
        var mask2 = Vector256.Create(0x003f03f0).AsByte();
        var shift2 = Vector256.Create(0x01000010).AsUInt16();
        var const51 = Vector256.Create((byte)51);
        var const25 = Vector256.Create((byte)25);
        var shuffleVector = Vector256.Create(
            (byte)5, 4, 6, 5, 8, 7, 9, 8, 11, 10, 12, 11, 14, 13, 15, 14,
            1, 0, 2, 1, 4, 3, 5, 4, 7, 6, 8, 7, 10, 9, 11, 10);
        var offsetMap = Vector256.Create(
            (sbyte)65, 71, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -19, -16, 0, 0,
            65, 71, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -19, -16, 0, 0).AsByte();

        if (inputLength >= 32)
        {
            fixed (byte* bytesPtr = bytes)
            fixed (short* charsPtr = MemoryMarshal.Cast<char, short>(chars))
            {
                var currentInputPtr = bytesPtr;
                var currentOutputPtr = charsPtr;
                Vector256<byte> inputVector;
                var preInputVector = Avx2.LoadVector256(currentInputPtr);
                currentInputPtr -= 4;
                inputVector = Avx2.PermuteVar8x32(preInputVector.AsInt32(), permuter).AsByte();
            MainLoop:
                var inputWithRepeat = Avx2.Shuffle(inputVector, shuffleVector);
                var masked1 = Avx2.And(inputWithRepeat, mask1);
                var maskedAndShifted1 = Avx2.MultiplyHigh(masked1.AsUInt16(), shift1);
                var masked2 = Avx2.And(inputWithRepeat, mask2);
                var maskedAndShifted2 = Avx2.MultiplyLow(masked2.AsUInt16(), shift2);
                var shuffled = Avx2.Or(maskedAndShifted1, maskedAndShifted2).AsByte();

                var shuffleResult = Avx2.SubtractSaturate(shuffled, const51);
                var less = Avx2.CompareGreaterThan(shuffled.AsSByte(), const25.AsSByte()).AsByte();
                shuffleResult = Avx2.Subtract(shuffleResult, less);
                var offsets = Avx2.Shuffle(offsetMap, shuffleResult);
                var translated = Avx2.Add(offsets, shuffled);

                var lower = translated.GetLower();
                var lowerInterleaved = Avx2.ConvertToVector256Int16(lower);
                Avx2.Store(currentOutputPtr, lowerInterleaved);
                currentOutputPtr += 16;
                var upper = translated.GetUpper();
                var upperInterleaved = Avx2.ConvertToVector256Int16(upper);
                Avx2.Store(currentOutputPtr, upperInterleaved);
                currentOutputPtr += 16;

                currentInputPtr += 24;
                inputLength -= 24;
                if (inputLength >= 28)
                {
                    inputVector = Avx2.LoadVector256(currentInputPtr);
                    goto MainLoop;
                }

                charsWritten = (int)(currentOutputPtr - charsPtr);
            }
        }

        var result = System.Convert.TryToBase64Chars(bytes[^inputLength..], chars[charsWritten..], out var charsWritten2);
        charsWritten += charsWritten2;
        return result;
    }


    private static unsafe bool TryFromBase64Chars(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten)
    {
        var inputLength = chars.Length;
        var outputLength = bytes.Length;
        bytesWritten = 0;
        if (inputLength >= 34)
        {
            var utf8mask = Vector256.Create((ushort)0xff00).AsInt16();
            var const2f = Vector256.Create((byte)0x2f);
            var lutLo = Vector256.Create(
                (byte)21, 17, 17, 17, 17, 17, 17, 17, 17, 17, 19, 26, 27, 27, 27, 26,
                21, 17, 17, 17, 17, 17, 17, 17, 17, 17, 19, 26, 27, 27, 27, 26);
            var lutHi = Vector256.Create(
                (byte)16, 16, 1, 2, 4, 8, 4, 8, 16, 16, 16, 16, 16, 16, 16, 16,
                16, 16, 1, 2, 4, 8, 4, 8, 16, 16, 16, 16, 16, 16, 16, 16);
            var lutRoll = Vector256.Create(
                (sbyte)0, 16, 19, 4, -65, -65, -71, -71, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 16, 19, 4, -65, -65, -71, -71, 0, 0, 0, 0, 0, 0, 0, 0);
            var helper1 = Vector256.Create(0x01400140).AsSByte();
            var helper2 = Vector256.Create(0x00011000);
            var helper3 = Vector256.Create(
                (sbyte)2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, -1,
                2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, -1);
            var helper4 = Vector256.Create(0, 1, 2, 4, 5, 6, -1, -1);
            fixed (byte* bytesPtr = bytes)
            fixed (short* charsPtr = MemoryMarshal.Cast<char, short>(chars))
            {
                var currentBytesPtr = bytesPtr;
                var currentInputPtr = charsPtr;

                while (inputLength >= 34 && outputLength >= 32)
                {
                    var input1 = Avx2.LoadVector256(currentInputPtr);
                    if (!Avx2.TestZ(input1, utf8mask))
                    {
                        bytesWritten = 0;
                        return false;
                    }

                    var input2 = Avx2.LoadVector256(currentInputPtr + 16);
                    if (!Avx2.TestZ(input2, utf8mask))
                    {
                        bytesWritten = 0;
                        return false;
                    }

                    currentInputPtr += 32;
                    inputLength -= 32;

                    var packedInput = Avx2.PackUnsignedSaturate(input1, input2);
                    var input = Avx2.Permute4x64(packedInput.AsUInt64(), (byte)0b_11_01_10_00).AsByte();

                    var hiNibbles = Avx2.ShiftRightLogical(input.AsInt32(), 4).AsByte();
                    var loNibbles = Avx2.And(input, const2f);
                    var lo = Avx2.Shuffle(lutLo, loNibbles);
                    var eq2f = Avx2.CompareEqual(input, const2f);
                    hiNibbles = Avx2.And(hiNibbles, const2f);
                    var hi = Avx2.Shuffle(lutHi, hiNibbles);
                    var roll = Avx2.Shuffle(lutRoll, Avx2.Add(eq2f, hiNibbles).AsSByte());
                    if (!Avx2.TestZ(lo, hi))
                    {
                        bytesWritten = 0;
                        return false;
                    }

                    var fromAscii = Avx2.Add(input.AsSByte(), roll);

                    var mergeXYandZT = Avx2.MultiplyAddAdjacent(fromAscii.AsByte(), helper1);
                    var packedWithinLanes = Avx2.MultiplyAddAdjacent(mergeXYandZT, helper2.AsInt16());
                    packedWithinLanes = Avx2.Shuffle(packedWithinLanes.AsByte(), helper3.AsByte()).AsInt32();
                    var final = Avx2.PermuteVar8x32(packedWithinLanes, helper4).AsByte();
                    Avx2.Store(currentBytesPtr, final);

                    bytesWritten += 24;
                    currentBytesPtr += 24;
                    outputLength -= 24;
                }
            }
        }

        var result = System.Convert.TryFromBase64Chars(chars[^inputLength..], bytes[bytesWritten..], out var bytesWritten2);
        if (result)
        {
            bytesWritten += bytesWritten2;
        }
        else
        {
            bytesWritten = 0;
        }

        return result;
    }






}
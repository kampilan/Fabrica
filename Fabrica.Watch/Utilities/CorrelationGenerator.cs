using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Fabrica.Watch.Utilities;

public static class CorrelationGenerator
{

    private const uint Radix = 62;
    private const ulong Carry = 297528130221121800; // 2^64 / 62
    private const ulong CarryRemainder = 16;        // 2^64 % 62
    private const string Symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static void DivRem(ulong num, out ulong quot, out uint rem)
    {
        quot = num / Radix;
        rem = (uint)(num - (Radix * quot));
    }

    private static void DivRem(ulong numUpper64, ulong numLower64, out ulong quotUpper, out ulong quotLower, out uint rem)
    {
        uint remLower, remUpper;
        ulong remLowerQuot;

        DivRem(numUpper64, out quotUpper, out remUpper);
        DivRem(numLower64, out quotLower, out remLower);

        // take the upper remainder, and incorporate it into the other lower quotient/lower remainder/output remainder
        remLower += (uint)(remUpper * CarryRemainder); // max value = 61 + 61*16 = 1037
        DivRem(remLower, out remLowerQuot, out rem);

        // at this point the max values are:
        //   quotientLower: 2^64-17 / 62, which is 297528130221121799 (any more than 2^64-17 and remainderLower will be under 61)
        //   remainderUpper * carry: 61 * 297528130221121800 which is 18149215943488429800
        //   remainderLowerQuotient = 1037 / 62 = 16 
        quotLower += remUpper * Carry;  // max value is now 18446744073709551599
        quotLower += remLowerQuot;  // max value is now 18446744073709551615. So no overflow.
    }


    public static unsafe string New()
    {

        var bytes = NewGuid().ToByteArray();

        ulong lower, upper;

        lower = BitConverter.ToUInt64(bytes, 0);
        upper = BitConverter.ToUInt64(bytes, 8);

        var sb = stackalloc char[22]{'0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0' };
        int pos = 22;
        uint remainder;
        while (upper != 0)
        {
            DivRem(upper, lower, out upper, out lower, out remainder);
            sb[--pos] = Symbols[(int)remainder];
        }

        do
        {
            DivRem(lower, out lower, out remainder);
            sb[--pos] = Symbols[(int)remainder];
        } while (lower != 0);

        return new string(sb);

    }


    const int GUIDS_PER_THREAD = 1 << 8; // 256 (keep it power-of-2)
    const int GUID_SIZE_IN_BYTES = 16;

    struct Container
    {
        public Guid[] _guids;
        public byte _idx; // wraps around on 256 (GUIDS_PER_THREAD)
    }//Container

    [ThreadStatic] static Container ts_container; //ts stands for "ThreadStatic"

    /// <summary>Initializes a new instance of the <see cref="Guid"/> structure.</summary>
    /// <returns>A new <see cref="Guid"/> struct.</returns>
    /// <remarks>Faster alternative to <see cref="Guid.NewGuid"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid NewGuid()
    {
        ref Container container = ref ts_container;
        if (container._guids == null) container._guids = GC.AllocateUninitializedArray<Guid>(GUIDS_PER_THREAD); // more efficient than compound assignment
        ref Guid guid0 = ref MemoryMarshal.GetArrayDataReference(container._guids);
        byte idx = container._idx++;
        if (idx == 0)
        {
            RandomNumberGenerator.Fill(
                MemoryMarshal.CreateSpan<byte>(ref Unsafe.As<Guid, byte>(ref guid0), GUIDS_PER_THREAD * GUID_SIZE_IN_BYTES));
        }

        Guid guid = Unsafe.Add(ref guid0, idx);
        Unsafe.Add(ref guid0, idx) = default; // prevents Guid leakage
        return guid;
    }


}



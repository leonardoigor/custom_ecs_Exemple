using System;

namespace MiniECS
{
    public static class BitMask
    {
        public static void EnsureSize(ref ulong[] mask, int word)
        {
            if (mask == null)
            {
                mask = new ulong[Math.Max(1, word + 1)];
                return;
            }

            if (mask.Length > word) return;
            int newSize = Math.Max(word + 1, mask.Length == 0 ? 1 : mask.Length * 2);
            Array.Resize(ref mask, newSize);
        }

        public static bool HasNone(ulong[] mask, ulong[] forbidden)
        {
            if (forbidden == null) return true;
            for (int i = 0; i < forbidden.Length; i++)
                if ((mask[i] & forbidden[i]) != 0)
                    return false;
            return true;
        }
    }
}
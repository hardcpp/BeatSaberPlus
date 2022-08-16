#if CP_SDK_UNITY
namespace CP_SDK.Unity.OpenType
{
    public static class NumericHelpers
    {
        public static uint Log2(uint x)
        {
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);

            return (uint)(NumBitsSet(x) - 1);
        }

        public static uint NextPow2(uint x)
        {
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);

            return x + 1;
        }

        public static int NumBitsSet(uint x)
        {
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            x += (x >> 16);

            return (int)(x & 0x0000003f);
        }

    }
}
#endif

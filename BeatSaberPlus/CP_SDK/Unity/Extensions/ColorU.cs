using System.Runtime.CompilerServices;
using UnityEngine;

namespace CP_SDK.Unity.Extensions
{
    /// <summary>
    /// Unity Color tools
    /// </summary>
    public static class ColorU
    {
        private static char[] s_IntToHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get color with alpha
        /// </summary>
        /// <param name="p_Color">Source color</param>
        /// <param name="p_Alpha">Target alpha</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color WithAlpha(Color p_Color, float p_Alpha)
        {
            p_Color.a = p_Alpha;
            return p_Color;
        }
        /// <summary>
        /// Get color with alpha
        /// </summary>
        /// <param name="p_This">Source color</param>
        /// <param name="p_Alpha">Target alpha</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color WithAlpha(string p_Color, float p_Alpha)
        {
            var l_Color = ToUnityColor(p_Color);
            l_Color.a = p_Alpha;
            return l_Color;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Convert a Color to Color32
        /// </summary>
        /// <param name="p_Src">Input</param>
        /// <returns>Converted color</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Convert(Color32 p_Src)
        {
            return new Color(
               ((float)p_Src.r) / 255.0f,
               ((float)p_Src.g) / 255.0f,
               ((float)p_Src.b) / 255.0f,
               ((float)p_Src.a) / 255.0f
           );
        }
        /// <summary>
        /// Convert a Color32 to Color
        /// </summary>
        /// <param name="p_Src">Input</param>
        /// <returns>Converted color</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 Convert(Color p_Src)
        {
            return new Color32(
                (byte)(p_Src.r * 255.0f),
                (byte)(p_Src.g * 255.0f),
                (byte)(p_Src.b * 255.0f),
                (byte)(p_Src.a * 255.0f)
            );
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// String to unity color
        /// </summary>
        /// <param name="p_Src">Input string</param>
        /// <param name="p_Color">Output color</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToUnityColor(string p_Src, out Color p_Color)
        {
            p_Color = Color.black;
            if (p_Src == null)
                return false;

            var l_Length = p_Src.Length;
            if (l_Length == 0)
                return false;

            var l_Offset = p_Src[0] == '#' ? 1 : 0;
            var l_R = (ConvertSingleByte(p_Src, l_Length, l_Offset + 0) << 4) | ConvertSingleByte(p_Src, l_Length, l_Offset + 1);
            var l_G = (ConvertSingleByte(p_Src, l_Length, l_Offset + 2) << 4) | ConvertSingleByte(p_Src, l_Length, l_Offset + 3);
            var l_B = (ConvertSingleByte(p_Src, l_Length, l_Offset + 4) << 4) | ConvertSingleByte(p_Src, l_Length, l_Offset + 5);
            var l_A = 255;

            if ((l_Length - l_Offset) > 6)
                l_A = (ConvertSingleByte(p_Src, l_Length, l_Offset + 6) << 4) | ConvertSingleByte(p_Src, l_Length, l_Offset + 7);

            p_Color = Convert(new Color32((byte)l_R, (byte)l_G, (byte)l_B, (byte)l_A));

            return true;
        }
        /// <summary>
        /// String to unity color
        /// </summary>
        /// <param name="p_Src">Input string</param>
        /// <returns>Output color</returns>
        public static Color ToUnityColor(string p_Src)
        {
            TryToUnityColor(p_Src, out var l_Color);
            return l_Color;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// To hexadecimal RGB with # prefix
        /// </summary>
        /// <param name="p_Color">Input color</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHexRGB(Color p_Color)
        {
            var l_Color32 = Convert(p_Color);
            return new string(new char[]
            {
                '#',
                s_IntToHex[(l_Color32.r >> 4) & 0xF],
                s_IntToHex[(l_Color32.r >> 0) & 0xF],
                s_IntToHex[(l_Color32.g >> 4) & 0xF],
                s_IntToHex[(l_Color32.g >> 0) & 0xF],
                s_IntToHex[(l_Color32.b >> 4) & 0xF],
                s_IntToHex[(l_Color32.b >> 0) & 0xF]
            });
        }
        /// <summary>
        /// To hexadecimal RGBA with # prefix
        /// </summary>
        /// <param name="p_Color">Input color</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHexRGBA(Color p_Color)
        {
            var l_Color32 = Convert(p_Color);
            return new string(new char[]
            {
                '#',
                s_IntToHex[(l_Color32.r >> 4) & 0xF],
                s_IntToHex[(l_Color32.r >> 0) & 0xF],
                s_IntToHex[(l_Color32.g >> 4) & 0xF],
                s_IntToHex[(l_Color32.g >> 0) & 0xF],
                s_IntToHex[(l_Color32.b >> 4) & 0xF],
                s_IntToHex[(l_Color32.b >> 0) & 0xF],
                s_IntToHex[(l_Color32.a >> 4) & 0xF],
                s_IntToHex[(l_Color32.a >> 0) & 0xF]
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Convert single hex byte to integer
        /// </summary>
        /// <param name="p_Src">Source string</param>
        /// <param name="p_Size">Source size</param>
        /// <param name="p_Pos">Position to convert</param>
        /// <returns>Converted integer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ConvertSingleByte(string p_Src, int p_Size, int p_Pos)
        {
            if (p_Pos >= p_Size)
                return 0;

            var l_Char = p_Src[p_Pos];
            if (l_Char >= '0' && l_Char <= '9')
                return (l_Char - '0') + 0x00;
            else if (l_Char >= 'a' && l_Char <= 'f')
                return (l_Char - 'a') + 0x0A;
            else if (l_Char >= 'A' && l_Char <= 'F')
                return (l_Char - 'A') + 0x0A;

            return 0;
        }
    }
}
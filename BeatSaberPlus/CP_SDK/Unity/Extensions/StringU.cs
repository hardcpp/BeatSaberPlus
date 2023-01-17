using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.Unity.Extensions
{
    /// <summary>
    /// String utilities
    /// </summary>
    public static class StringU
    {
        /// <summary>
        /// Look up table
        /// </summary>
        private static readonly Dictionary<char, byte> m_LookupTable = new Dictionary<char, byte>()
        {
            ['0'] = 0x0,
            ['1'] = 0x1,
            ['2'] = 0x2,
            ['3'] = 0x3,
            ['4'] = 0x4,
            ['5'] = 0x5,
            ['6'] = 0x6,
            ['7'] = 0x7,
            ['8'] = 0x8,
            ['9'] = 0x9,
            ['a'] = 0xA, ['A'] = 0xA,
            ['b'] = 0xB, ['B'] = 0xB,
            ['c'] = 0xC, ['C'] = 0xC,
            ['d'] = 0xD, ['D'] = 0xD,
            ['e'] = 0xE, ['E'] = 0xE,
            ['f'] = 0xF, ['F'] = 0xF,
        };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// String to unity color
        /// </summary>
        /// <param name="p_This"></param>
        /// <returns></returns>
        public static Color ToUnityColor(this string p_This)
        {
            if (string.IsNullOrEmpty(p_This))
                return Color.black;

            var l_Offset = 0;
            if (p_This[0] == '#')
                l_Offset++;

            var l_R = (m_LookupTable[p_This[l_Offset + 0]] << 4) | m_LookupTable[p_This[l_Offset + 1]];
            var l_G = (m_LookupTable[p_This[l_Offset + 2]] << 4) | m_LookupTable[p_This[l_Offset + 3]];
            var l_B = (m_LookupTable[p_This[l_Offset + 4]] << 4) | m_LookupTable[p_This[l_Offset + 5]];
            var l_A = 255;

            if ((p_This.Length - l_Offset) == 8)
                l_A = (m_LookupTable[p_This[l_Offset + 6]] << 4) | m_LookupTable[p_This[l_Offset + 7]];

            return new Color32((byte)l_R, (byte)l_G, (byte)l_B, (byte)l_A);
        }
    }
}

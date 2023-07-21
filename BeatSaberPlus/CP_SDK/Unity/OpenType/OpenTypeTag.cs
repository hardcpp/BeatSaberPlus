#if CP_SDK_UNITY
using System;
using System.Linq;

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType Tag element
    /// </summary>
    public struct OpenTypeTag
    {
        public static readonly OpenTypeTag NAME = FromString("name");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public byte[] Value { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor from char16 array
        /// </summary>
        /// <param name="p_Chars">Input char</param>
        /// <returns></returns>
        public static OpenTypeTag FromChars(char[] p_Chars)
            => new OpenTypeTag(p_Chars.Select(c => (byte)c).ToArray());
        /// <summary>
        /// Constructor from string
        /// </summary>
        /// <param name="p_String">Input string</param>
        /// <returns></returns>
        public static OpenTypeTag FromString(string p_String)
            => FromChars(p_String.ToCharArray(0, 4));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Value">Input value</param>
        public OpenTypeTag(byte[] p_Value)
            => Value = p_Value;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get as integer
        /// </summary>
        /// <returns></returns>
        public uint AsInt()
            => BitConverter.ToUInt32(Value, 0);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is equal operator
        /// </summary>
        /// <param name="p_Other">Element to compare</param>
        /// <returns></returns>
        public override bool Equals(object p_Other)
            => p_Other is OpenTypeTag l_OtherTag && AsInt() == l_OtherTag.AsInt();
        /// <summary>
        /// Is equal operator
        /// </summary>
        /// <param name="p_Left">Element to compare</param>
        /// <param name="p_Right">Element to compare</param>
        /// <returns></returns>
        public static bool operator ==(OpenTypeTag p_Left, OpenTypeTag p_Right)
            => p_Left.Equals(p_Right);
        /// <summary>
        /// Is equal different
        /// </summary>
        /// <param name="p_Left">Element to compare</param>
        /// <param name="p_Right">Element to compare</param>
        /// <returns></returns>
        public static bool operator !=(OpenTypeTag p_Left, OpenTypeTag p_Right)
            => !(p_Left == p_Right);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get hash code of this instance
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => 1637310455 + AsInt().GetHashCode();
    }
}
#endif

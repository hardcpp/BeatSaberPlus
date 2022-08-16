#if CP_SDK_UNITY
using System;
using System.Linq;

namespace CP_SDK.Unity.OpenType
{
    public struct OpenTypeTag
    {
        public static readonly OpenTypeTag NAME = FromString("name");

        public byte[] Value { get; set; }
        public uint IntValue => BitConverter.ToUInt32(Value, 0);

        public bool Validate()
            => Value.Length == 4 && Value.All(b => b >= 0x20 && b <= 0x7E);

        public override bool Equals(object obj)
            => obj is OpenTypeTag tag && IntValue == tag.IntValue;

        public override int GetHashCode()
            => 1637310455 + IntValue.GetHashCode();

        public OpenTypeTag(byte[] value) => Value = value;

        public static OpenTypeTag FromChars(char[] chrs)
            => new OpenTypeTag(chrs.Select(c => (byte)c).ToArray());
        public static OpenTypeTag FromString(string str)
            => FromChars(str.ToCharArray(0, 4));

        public static bool operator ==(OpenTypeTag left, OpenTypeTag right)
            => left.Equals(right);

        public static bool operator !=(OpenTypeTag left, OpenTypeTag right)
            => !(left == right);
    }
}
#endif

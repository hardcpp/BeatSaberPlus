#if CP_SDK_UNITY
using System;
using System.IO;
using System.Text;

namespace CP_SDK.Unity.OpenType
{
    public abstract class OpenTypeReader : BinaryReader
    {
        protected OpenTypeReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public static byte[] FromBigEndian(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public new ushort ReadUInt16()
            => BitConverter.ToUInt16(FromBigEndian(ReadBytes(2)), 0);
        public new uint ReadUInt32()
            => BitConverter.ToUInt32(FromBigEndian(ReadBytes(4)), 0);

        public OpenTypeTag ReadTag() => new OpenTypeTag(ReadBytes(4));

        public ushort ReadOffset16() => ReadUInt16();
        public uint ReadOffset32() => ReadUInt32();


        public static OpenTypeReader For(Stream stream, Encoding enc = null, bool leaveOpen = false)
        {
            if (enc == null)
                enc = Encoding.Default;

            var start = stream.Position;
            var reader = new BinaryReader(stream, enc, true);
            var tag = BitConverter.ToUInt32(FromBigEndian(reader.ReadBytes(4)), 0);
            stream.Position = start;

            if (tag == CollectionHeader.TTCTagBEInt) return new OpenTypeCollectionReader(stream, enc, leaveOpen);
            if (tag == OffsetTable.OpenTypeCFFVersion) return new OpenTypeFontReader(stream, enc, leaveOpen);
            if (tag == OffsetTable.TrueTypeOnlyVersion) return new OpenTypeFontReader(stream, enc, leaveOpen);

            return null;
        }
    }
}
#endif

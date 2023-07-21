#if CP_SDK_UNITY
using System;
using System.IO;
using System.Text;

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType abstract reader
    /// </summary>
    public abstract class OpenTypeReader : BinaryReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Stream">Input stream</param>
        /// <param name="p_Encoding">Stream encoding</param>
        /// <param name="p_LeaveOpen">Should leave stream open?</param>
        protected OpenTypeReader(Stream p_Stream, Encoding p_Encoding, bool p_LeaveOpen)
            : base(p_Stream, p_Encoding, p_LeaveOpen)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public new ushort ReadUInt16()
            => BitConverter.ToUInt16(FromBigEndian(ReadBytes(2)), 0);
        public new uint ReadUInt32()
            => BitConverter.ToUInt32(FromBigEndian(ReadBytes(4)), 0);
        public OpenTypeTag ReadTag()
            => new OpenTypeTag(ReadBytes(4));
        public ushort ReadOffset16()
            => ReadUInt16();
        public uint ReadOffset32()
            => ReadUInt32();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// From big endian
        /// </summary>
        /// <param name="p_Bytes">Bytes</param>
        /// <returns></returns>
        public static byte[] FromBigEndian(byte[] p_Bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(p_Bytes);

            return p_Bytes;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Stream">Input stream</param>
        /// <param name="p_Encoding">Stream encoding</param>
        /// <param name="p_LeaveOpen">Should leave stream open?</param>
        public static OpenTypeReader For(Stream p_Stream, Encoding p_Encoding = null, bool p_LeaveOpen = false)
        {
            if (p_Encoding == null)
                p_Encoding = Encoding.Default;

            var l_Start     = p_Stream.Position;
            var l_Reader    = new BinaryReader(p_Stream, p_Encoding, true);
            var l_Tag       = BitConverter.ToUInt32(FromBigEndian(l_Reader.ReadBytes(4)), 0);

            p_Stream.Position = l_Start;

            if (l_Tag == CollectionHeader.TTCTagBEInt)          return new OpenTypeCollectionReader(p_Stream, p_Encoding, p_LeaveOpen);
            if (l_Tag == OffsetTable.OPEN_TYPE_CFF_VERSION)     return new OpenTypeFontReader(p_Stream, p_Encoding, p_LeaveOpen);
            if (l_Tag == OffsetTable.TRUE_TYPE_ONLY_VERSION)    return new OpenTypeFontReader(p_Stream, p_Encoding, p_LeaveOpen);

            return null;
        }
    }
}
#endif

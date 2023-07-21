#if CP_SDK_UNITY
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType name table
    /// </summary>
    public class OpenTypeNameTable : OpenTypeTable
    {
        /// <summary>
        /// Lang tab record
        /// </summary>
        public class LangTagRecord
        {
            public const uint   Size = 4;
            public ushort       Length { get; set; }
            public ushort       Offset { get; set; }
            public string       Value { get; set; }
        }
        /// <summary>
        /// Format
        /// </summary>
        public enum EFormat : ushort
        {
            Default     = 0,
            LangTagged  = 1
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public EFormat                      Format          { get; private set; }
        public ushort                       Count           { get; private set; }
        public ushort                       StringOffset    { get; private set; }
        public IReadOnlyList<NameRecord>    NameRecords     { get; private set; }
        public ushort                       LangTagCount    { get; private set; } = 0;
        public IReadOnlyList<LangTagRecord> LangTagRecords  { get; private set; } = new List<LangTagRecord>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read from
        /// </summary>
        /// <param name="p_Reader">Reader</param>
        /// <param name="p_Length">Size to read</param>
        public override void ReadFrom(OpenTypeReader p_Reader, uint p_Length)
        {
            Format          = (EFormat)p_Reader.ReadUInt16();
            Count           = p_Reader.ReadUInt16();
            StringOffset    = p_Reader.ReadUInt16();

            var l_SeekOffset = (uint)StringOffset - 6;
            NameRecords = ReadNameRecords(p_Reader);
            l_SeekOffset -= Count * NameRecord.SIZE;

            if (Format == EFormat.LangTagged)
            {
                LangTagCount = p_Reader.ReadUInt16();
                l_SeekOffset -= 2;
                LangTagRecords = ReadLangTagRecords(p_Reader);
                l_SeekOffset -= LangTagCount * LangTagRecord.Size;
            }

            p_Reader.BaseStream.Seek(l_SeekOffset, SeekOrigin.Current);
            var l_StartBase = p_Reader.BaseStream.Position;

            foreach (var l_CurrentName in NameRecords)
            {
                p_Reader.BaseStream.Position = l_StartBase + l_CurrentName.Offset;
                var l_NameBytes = p_Reader.ReadBytes(l_CurrentName.Length);

                /// TODO: maybe know how to identify more platforms and encodings?
                if (l_CurrentName.PlatformID == NameRecord.EPlatform.Windows || l_CurrentName.PlatformID == NameRecord.EPlatform.Unicode)
                    l_CurrentName.Value = Encoding.BigEndianUnicode.GetString(l_NameBytes);
                else
                    l_CurrentName.Value = Encoding.UTF8.GetString(l_NameBytes); ///< hope and pray that the encoding is always UTF-8
            }

            foreach (var l_CurrentLangTag in LangTagRecords)
            {
                p_Reader.BaseStream.Position = l_StartBase + l_CurrentLangTag.Offset;
                var l_NameBytes = p_Reader.ReadBytes(l_CurrentLangTag.Length);

                l_CurrentLangTag.Value = Encoding.UTF8.GetString(l_NameBytes);  ///< hope and pray that the encoding is always UTF-8
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read name records
        /// </summary>
        /// <param name="p_Reader">Stream</param>
        /// <returns></returns>
        private IReadOnlyList<NameRecord> ReadNameRecords(OpenTypeReader p_Reader)
        {
            var l_List = new List<NameRecord>(Count);
            for (var l_I = 0; l_I < Count; ++l_I)
            {
                l_List.Add(new NameRecord
                {
                    PlatformID  = (NameRecord.EPlatform)p_Reader.ReadUInt16(),
                    EncodingID  = p_Reader.ReadUInt16(),
                    LanguageID  = p_Reader.ReadUInt16(),
                    NameID      = (NameRecord.ENameType)p_Reader.ReadUInt16(),
                    Length      = p_Reader.ReadUInt16(),
                    Offset      = p_Reader.ReadOffset16()
                });
            }
            return l_List;
        }
        /// <summary>
        /// Read lang tab records
        /// </summary>
        /// <param name="p_Reader">Stream</param>
        /// <returns></returns>
        private IReadOnlyList<LangTagRecord> ReadLangTagRecords(OpenTypeReader p_Reader)
        {
            var l_List = new List<LangTagRecord>(LangTagCount);
            for (var l_I = 0; l_I < LangTagCount; ++l_I)
            {
                l_List.Add(new LangTagRecord
                {
                    Length = p_Reader.ReadUInt16(),
                    Offset = p_Reader.ReadOffset16()
                });
            }

            return l_List;
        }
    }
}
#endif
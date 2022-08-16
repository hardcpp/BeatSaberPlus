#if CP_SDK_UNITY
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CP_SDK.Unity.OpenType
{
    public abstract class OpenTypeTable
    {
        public abstract void ReadFrom(OpenTypeReader reader, uint length);
    }

    public class OpenTypeNameTable : OpenTypeTable
    {
        public enum FormatEnum : ushort
        {
            Default = 0, LangTagged = 1
        }

        public FormatEnum Format { get; private set; }

        public ushort Count { get; private set; }
        /// <summary>
        /// Offset to start of string storage from table start
        /// </summary>
        public ushort StringOffset { get; private set; }
        public IReadOnlyList<NameRecord> NameRecords { get; private set; }

        public ushort LangTagCount { get; private set; } = 0;
        public IReadOnlyList<LangTagRecord> LangTagRecords { get; private set; } = new List<LangTagRecord>();

        public override void ReadFrom(OpenTypeReader reader, uint length)
        {
            Format = (FormatEnum)reader.ReadUInt16();
            Count = reader.ReadUInt16();
            StringOffset = reader.ReadUInt16();
            uint seekOffset = (uint)StringOffset - 6;
            NameRecords = ReadNameRecords(reader);
            seekOffset -= Count * NameRecord.Size;
            if (Format == FormatEnum.LangTagged)
            {
                LangTagCount = reader.ReadUInt16();
                seekOffset -= 2;
                LangTagRecords = ReadLangTagRecords(reader);
                seekOffset -= LangTagCount * LangTagRecord.Size;
            }

            reader.BaseStream.Seek(seekOffset, SeekOrigin.Current);
            var startBase = reader.BaseStream.Position;


            foreach (var name in NameRecords)
            {
                reader.BaseStream.Position = startBase + name.Offset;
                var nameBytes = reader.ReadBytes(name.Length);

                // TODO: maybe know how to identify more platforms and encodings?
                if (name.PlatformID == NameRecord.Platform.Windows || name.PlatformID == NameRecord.Platform.Unicode)
                {
                    name.Value = Encoding.BigEndianUnicode.GetString(nameBytes);
                }
                else
                {
                    // hope and pray that the encoding is always UTF-8
                    name.Value = Encoding.UTF8.GetString(nameBytes);
                }
            }

            foreach (var langTag in LangTagRecords)
            {
                reader.BaseStream.Position = startBase + langTag.Offset;
                var nameBytes = reader.ReadBytes(langTag.Length);

                // hope and pray that the encoding is always UTF-8
                langTag.Value = Encoding.UTF8.GetString(nameBytes);
            }
        }

        // uses Count to read them
        private IReadOnlyList<NameRecord> ReadNameRecords(OpenTypeReader reader)
        {
            var list = new List<NameRecord>();
            for (int i = 0; i < Count; i++)
            {
                list.Add(new NameRecord
                {
                    PlatformID = (NameRecord.Platform)reader.ReadUInt16(),
                    EncodingID = reader.ReadUInt16(),
                    LanguageID = reader.ReadUInt16(),
                    NameID = (NameRecord.NameType)reader.ReadUInt16(),
                    Length = reader.ReadUInt16(),
                    Offset = reader.ReadOffset16()
                });
            }
            return list;
        }

        public class NameRecord
        {
            public const uint Size = 12;
            public const ushort USEnglishLangID = 0x0409;

            public enum Platform : ushort
            {
                Unicode = 0, Macintosh = 1, ISO = 2,
                Windows = 3, Custom = 4
            }

            public Platform PlatformID { get; set; }

            public ushort EncodingID { get; set; }
            public ushort LanguageID { get; set; }

            public enum NameType : ushort
            {
                Copyright = 0, FontFamily = 1, FontSubfamily = 2,
                UniqueId = 3, FullFontName = 4, Version = 5,
                PostScriptName = 6, Trademark = 7, Manufacturer = 8,
                Designer = 9, Description = 10, VendorURL = 11,
                DesignerURL = 12, LicenseDescription = 13,
                LicenseInfoURL = 14, Reserved1 = 15,
                TypographicFamily = 16,
                TypographicSubfamily = 17,
                /// <summary>
                /// This is a Macintosh only field.
                /// </summary>
                CompatibleFull = 18,
                SampleText = 19, PostScriptCID = 20,
                WWSFamily = 21, WWSSubfamily = 22,
                LightBackgroundPalette = 23,
                DarkBackgroundPalette = 24,
                VariationsPostScriptPrefix = 25,
            }

            public NameType NameID { get; set; }
            public ushort Length { get; set; }
            public ushort Offset { get; set; }

            public string Value { get; set; }
        }

        // uses LangTagCount to read them
        private IReadOnlyList<LangTagRecord> ReadLangTagRecords(OpenTypeReader reader)
        {
            var list = new List<LangTagRecord>();
            for (int i = 0; i < LangTagCount; i++)
            {
                list.Add(new LangTagRecord
                {
                    Length = reader.ReadUInt16(),
                    Offset = reader.ReadOffset16()
                });
            }
            return list;
        }

        public class LangTagRecord
        {
            public const uint Size = 4;
            public ushort Length { get; set; }
            /// <summary>
            /// string offset from start of storage area
            /// </summary>
            public ushort Offset { get; set; }

            public string Value { get; set; }
        }
    }
}
#endif

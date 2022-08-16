#if CP_SDK_UNITY
using System;
using System.Collections.Generic;
using System.Linq;

namespace CP_SDK.Unity.OpenType
{
    internal class OpenTypeFont
    {
        private readonly OffsetTable offsetTable;

        private readonly TableRecord[] tables;
        private readonly TableRecord? nameTableRecord;

        internal OpenTypeFontReader Reader { get; }

        internal OpenTypeFont(OpenTypeFontReader reader, bool lazyLoad = true) : this(reader.ReadOffsetTable(), reader, lazyLoad)
        {
        }

        internal OpenTypeFont(OffsetTable offsets, OpenTypeFontReader reader, bool lazyLoad = true)
        {
            offsetTable = offsets;
            tables = reader.ReadTableRecords(offsetTable);
            nameTableRecord = tables.Select(t => new TableRecord?(t))
                .FirstOrDefault(t => t.Value.TableTag == OpenTypeTag.NAME);

            if (lazyLoad)
                Reader = reader;
            else
                LoadAllTables(reader);
        }

        private void LoadAllTables(OpenTypeFontReader reader)
        {
            nameTable = ReadNameTable(reader);
            // TODO: do something with this
        }

        private OpenTypeNameTable nameTable = null;
        internal OpenTypeNameTable NameTable
        {
            get
            {
                if (nameTable == null) nameTable = ReadNameTable(Reader);
                return nameTable;
            }
        }

        private string uniqueId = null;
        internal string UniqueId
        {
            get
            {
                if (uniqueId == null) uniqueId = FindBestNameRecord(OpenTypeNameTable.NameRecord.NameType.UniqueId)?.Value;
                return uniqueId;
            }
        }

        private string family = null;
        internal string Family
        {
            get
            {
                if (family == null) family = FindBestNameRecord(OpenTypeNameTable.NameRecord.NameType.FontFamily)?.Value;
                return family;
            }
        }

        private string subfamily = null;
        internal string Subfamily
        {
            get
            {
                if (subfamily == null) subfamily = FindBestNameRecord(OpenTypeNameTable.NameRecord.NameType.FontSubfamily)?.Value;
                return subfamily;
            }
        }

        private string fullName = null;
        internal string FullName
        {
            get
            {
                if (fullName == null) fullName = FindBestNameRecord(OpenTypeNameTable.NameRecord.NameType.FullFontName)?.Value;
                return fullName;
            }
        }


        private OpenTypeNameTable ReadNameTable(OpenTypeFontReader reader)
            => reader.TryReadTable(nameTableRecord.Value) as OpenTypeNameTable;

        private OpenTypeNameTable.NameRecord FindBestNameRecord(OpenTypeNameTable.NameRecord.NameType type)
        {
            int RankPlatform(OpenTypeNameTable.NameRecord record)
            {
                if (record.PlatformID == OpenTypeNameTable.NameRecord.Platform.Windows) return 3000;
                if (record.PlatformID == OpenTypeNameTable.NameRecord.Platform.Unicode) return 2000;
                if (record.PlatformID == OpenTypeNameTable.NameRecord.Platform.Macintosh) return 1000;

                return 0;
            }

            int RankLanguage(OpenTypeNameTable.NameRecord record)
            {
                if (record.PlatformID == OpenTypeNameTable.NameRecord.Platform.Windows
                    && record.LanguageID == OpenTypeNameTable.NameRecord.USEnglishLangID)
                    return 100;

                return 0;
            }

            return NameTable.NameRecords.Where(r => r.NameID == type)
                .OrderByDescending(r => RankPlatform(r) + RankLanguage(r))
                .FirstOrDefault();
        }

        internal IEnumerable<TableRecord> Tables => tables;
    }
}
#endif

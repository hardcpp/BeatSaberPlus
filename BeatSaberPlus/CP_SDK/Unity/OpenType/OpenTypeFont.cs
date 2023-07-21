#if CP_SDK_UNITY
using System.Linq;

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType font
    /// </summary>
    public class OpenTypeFont
    {
        private readonly OffsetTable    m_OffsetTable;
        private readonly TableRecord[]  m_Tables;
        private readonly TableRecord?   m_NameTableRecord;

        private OpenTypeNameTable   m_NameTable = null;
        private string              m_UniqueID  = null;
        private string              m_Family    = null;
        private string              m_SubFamily = null;
        private string              m_FullName  = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public OpenTypeFontReader Reader { get; }

        internal OpenTypeNameTable NameTable { get {
                if (m_NameTable == null) m_NameTable = ReadNameTable(Reader);
                return m_NameTable;
        } }
        internal string UniqueID { get {
            if (m_UniqueID == null)     m_UniqueID  = FindBestNameRecord(NameRecord.ENameType.UniqueId)?.Value;
            return m_UniqueID;
        } }
        internal string Family { get {
            if (m_Family == null)       m_Family    = FindBestNameRecord(NameRecord.ENameType.FontFamily)?.Value;
            return m_Family;
        } }
        internal string Subfamily { get {
            if (m_SubFamily == null)    m_SubFamily = FindBestNameRecord(NameRecord.ENameType.FontSubfamily)?.Value;
            return m_SubFamily;
        } }
        internal string FullName { get {
                if (m_FullName == null) m_FullName  = FindBestNameRecord(NameRecord.ENameType.FullFontName)?.Value;
                return m_FullName;
        } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Reader">Reader instance</param>
        /// <param name="p_LazyLoad">Should lazy load?</param>
        public OpenTypeFont(OpenTypeFontReader p_Reader, bool p_LazyLoad = true)
            : this(p_Reader.ReadOffsetTable(), p_Reader, p_LazyLoad)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Offsets">Offset table</param>
        /// <param name="p_Reader">Reader instance</param>
        /// <param name="p_LazyLoad">Should lazy load?</param>
        public OpenTypeFont(OffsetTable p_Offsets, OpenTypeFontReader p_Reader, bool p_LazyLoad = true)
        {
            m_OffsetTable       = p_Offsets;
            m_Tables            = p_Reader.ReadTableRecords(m_OffsetTable);
            m_NameTableRecord   = m_Tables.Select(t => new TableRecord?(t)).FirstOrDefault(t => t.Value.TableTag == OpenTypeTag.NAME);

            if (p_LazyLoad)
                Reader = p_Reader;
            else
                LoadAllTables(p_Reader);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read name table
        /// </summary>
        /// <param name="p_Reader">Reader instance</param>
        /// <returns></returns>
        private OpenTypeNameTable ReadNameTable(OpenTypeFontReader p_Reader)
            => p_Reader.TryReadTable(m_NameTableRecord.Value) as OpenTypeNameTable;
        /// <summary>
        /// Find best name record
        /// </summary>
        /// <param name="p_Type">Name type</param>
        /// <returns></returns>
        private NameRecord FindBestNameRecord(NameRecord.ENameType p_Type)
        {
            int RankPlatform(NameRecord p_Record)
            {
                if (p_Record.PlatformID == NameRecord.EPlatform.Windows)   return 3000;
                if (p_Record.PlatformID == NameRecord.EPlatform.Unicode)   return 2000;
                if (p_Record.PlatformID == NameRecord.EPlatform.Macintosh) return 1000;

                return 0;
            }

            int RankLanguage(NameRecord p_Record)
            {
                if (p_Record.PlatformID == NameRecord.EPlatform.Windows && p_Record.LanguageID == NameRecord.USE_ENGLISH_LANG_ID)
                    return 100;

                return 0;
            }

            return NameTable.NameRecords.Where(r => r.NameID == p_Type)
                .OrderByDescending(r => RankPlatform(r) + RankLanguage(r))
                .FirstOrDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load all tables
        /// </summary>
        /// <param name="p_Reader">Reader</param>
        private void LoadAllTables(OpenTypeFontReader p_Reader)
        {
            m_NameTable = ReadNameTable(p_Reader);
        }
    }
}
#endif

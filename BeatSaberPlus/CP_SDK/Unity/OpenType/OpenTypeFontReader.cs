#if CP_SDK_UNITY
using System.IO;
using System.Text;

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType font reader
    /// </summary>
    public class OpenTypeFontReader : OpenTypeReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Stream">Input stream</param>
        /// <param name="p_Encoding">Stream encoding</param>
        /// <param name="p_LeaveOpen">Should leave stream open?</param>
        public OpenTypeFontReader(Stream p_Stream, Encoding p_Encoding, bool p_LeaveOpen)
            : base(p_Stream, p_Encoding, p_LeaveOpen)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read offset table
        /// </summary>
        /// <returns></returns>
        public OffsetTable ReadOffsetTable()
        {
            return new OffsetTable()
            {
                SFNTVersion     = ReadUInt32(),
                NumTables       = ReadUInt16(),
                SearchRange     = ReadUInt16(),
                EntrySelector   = ReadUInt16(),
                RangeShift      = ReadUInt16(),
                TablesStart     = BaseStream.Position,
            };
        }
        /// <summary>
        /// Read tables records
        /// </summary>
        /// <param name="p_Offsets">Offsets</param>
        /// <returns></returns>
        public TableRecord[] ReadTableRecords(OffsetTable p_Offsets)
        {
            BaseStream.Position = p_Offsets.TablesStart;

            var l_Tables = new TableRecord[p_Offsets.NumTables];
            for (int l_I = 0; l_I < p_Offsets.NumTables; ++l_I)
                l_Tables[l_I] = ReadTableRecord();

            return l_Tables;
        }
        /// <summary>
        /// Try read table
        /// </summary>
        /// <param name="p_Table">Table to read</param>
        /// <returns></returns>
        public OpenTypeTable TryReadTable(TableRecord p_Table)
        {
            BaseStream.Position = p_Table.Offset;

            var l_Result = null as OpenTypeTable;
            if (p_Table.TableTag == OpenTypeTag.NAME)
                l_Result = new OpenTypeNameTable();

            l_Result?.ReadFrom(this, p_Table.Length);

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read table record
        /// </summary>
        /// <returns></returns>
        protected TableRecord ReadTableRecord()
        {
            return new TableRecord()
            {
                TableTag    = ReadTag(),
                Checksum    = ReadUInt32(),
                Offset      = ReadOffset32(),
                Length      = ReadUInt32()
            };
        }
    }
}
#endif

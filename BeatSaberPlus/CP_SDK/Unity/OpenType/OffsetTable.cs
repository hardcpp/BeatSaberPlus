#if CP_SDK_UNITY
namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// Offset table
    /// </summary>
    public struct OffsetTable
    {
        public const uint TRUE_TYPE_ONLY_VERSION    = 0x00010000;
        public const uint OPEN_TYPE_CFF_VERSION     = 0x4F54544F;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public uint     SFNTVersion     { get; set; }
        public ushort   NumTables       { get; set; }
        public ushort   SearchRange     { get; set; }
        public ushort   EntrySelector   { get; set; }
        public ushort   RangeShift      { get; set; }
        public long     TablesStart     { get; set; }
    }
}
#endif

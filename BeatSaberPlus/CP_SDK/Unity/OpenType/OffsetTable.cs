#if CP_SDK_UNITY
namespace CP_SDK.Unity.OpenType
{
    public struct OffsetTable
    {
        public const uint TrueTypeOnlyVersion = 0x00010000;
        public const uint OpenTypeCFFVersion = 0x4F54544F;

        public uint SFNTVersion { get; set; }
        public ushort NumTables { get; set; }
        /// <summary>
        /// (Maximum power of 2 <= numTables) x 16.
        /// </summary>
        public ushort SearchRange { get; set; }
        /// <summary>
        /// Log2(maximum power of 2 <= numTables).
        /// </summary>
        public ushort EntrySelector { get; set; }
        /// <summary>
        /// NumTables x 16-searchRange.
        /// </summary>
        public ushort RangeShift { get; set; }

        public long TablesStart { get; set; }
    }
}
#endif

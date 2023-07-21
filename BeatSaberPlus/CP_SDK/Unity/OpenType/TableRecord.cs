#if CP_SDK_UNITY
namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// Table record
    /// </summary>
    public struct TableRecord
    {
        public OpenTypeTag  TableTag    { get; set; }
        public uint         Checksum    { get; set; }
        public uint         Offset      { get; set; }
        public uint         Length      { get; set; }
    }
}
#endif

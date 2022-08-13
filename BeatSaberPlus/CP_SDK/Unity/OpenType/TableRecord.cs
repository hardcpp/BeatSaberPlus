#if CP_SDK_UNITY
namespace CP_SDK.Unity.OpenType
{
    public struct TableRecord
    {
        public OpenTypeTag TableTag { get; set; }
        public uint Checksum { get; set; }
        /// <summary>
        /// Offset from the beginning of the file.
        /// </summary>
        public uint Offset { get; set; }
        public uint Length { get; set; }
    }
}
#endif

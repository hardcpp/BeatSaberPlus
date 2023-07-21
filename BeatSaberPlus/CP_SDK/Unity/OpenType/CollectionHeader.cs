#if CP_SDK_UNITY
namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// Collection header
    /// </summary>
    public struct CollectionHeader
    {
        public static readonly OpenTypeTag TCC_EXPECTED_TAG = OpenTypeTag.FromString("ttcf");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public const uint TTCTagBEInt   = 0x74746366;
        public const uint DSIGTagValue  = 0x44534947;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public OpenTypeTag  TTCTag          { get; set; }
        public ushort       MajorVersion    { get; set; }
        public ushort       MinorVersion    { get; set; }
        public uint         NumFonts        { get; set; }
        public uint[]       OffsetTable     { get; set; }
        public uint         DSIGTag         { get; set; }
        public uint         DSIGLength      { get; set; }
        public uint         DSIGOffset      { get; set; }
    }
}
#endif

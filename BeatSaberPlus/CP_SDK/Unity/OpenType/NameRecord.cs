#if CP_SDK_UNITY

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// Name record
    /// </summary>
    public class NameRecord
    {
        public const uint   SIZE                = 12;
        public const ushort USE_ENGLISH_LANG_ID = 0x0409;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public enum EPlatform : ushort
        {
            Unicode     = 0,
            Macintosh   = 1,
            ISO         = 2,
            Windows     = 3,
            Custom      = 4
        }

        public enum ENameType : ushort
        {
            Copyright                   = 0,
            FontFamily                  = 1,
            FontSubfamily               = 2,
            UniqueId                    = 3,
            FullFontName                = 4,
            Version                     = 5,
            PostScriptName              = 6,
            Trademark                   = 7,
            Manufacturer                = 8,
            Designer                    = 9,
            Description                 = 10,
            VendorURL                   = 11,
            DesignerURL                 = 12,
            LicenseDescription          = 13,
            LicenseInfoURL              = 14,
            Reserved1                   = 15,
            TypographicFamily           = 16,
            TypographicSubfamily        = 17,
            CompatibleFull              = 18,
            SampleText                  = 19,
            PostScriptCID               = 20,
            WWSFamily                   = 21,
            WWSSubfamily                = 22,
            LightBackgroundPalette      = 23,
            DarkBackgroundPalette       = 24,
            VariationsPostScriptPrefix  = 25,
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public EPlatform    PlatformID  { get; set; }
        public ushort       EncodingID  { get; set; }
        public ushort       LanguageID  { get; set; }
        public ENameType    NameID      { get; set; }
        public ushort       Length      { get; set; }
        public ushort       Offset      { get; set; }
        public string       Value       { get; set; }
    }

}
#endif
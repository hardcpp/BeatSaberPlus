#if CP_SDK_UNITY
using System.IO;
using System.Text;

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType collection reader
    /// </summary>
    internal class OpenTypeCollectionReader : OpenTypeFontReader
    {
        internal OpenTypeCollectionReader(Stream p_Stream, Encoding p_Encoding, bool p_LeaveOpen)
            : base(p_Stream, p_Encoding, p_LeaveOpen)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal CollectionHeader ReadCollectionHeader()
        {
            var l_Header = new CollectionHeader
            {
                TTCTag          = ReadTag(),
                MajorVersion    = ReadUInt16(),
                MinorVersion    = ReadUInt16(),
                NumFonts        = ReadUInt32(),
            };

            l_Header.OffsetTable = new uint[l_Header.NumFonts];
            for (uint l_I = 0; l_I < l_Header.NumFonts; ++l_I)
                l_Header.OffsetTable[l_I] = ReadOffset32();

            if (l_Header.MajorVersion == 2)
            {
                l_Header.DSIGTag    = ReadUInt32();
                l_Header.DSIGLength = ReadUInt32();
                l_Header.DSIGOffset = ReadUInt32();
            }

            return l_Header;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal OpenTypeFont[] ReadFonts(CollectionHeader p_Header, bool p_LazyLoad = true)
        {
            var l_Fonts = new OpenTypeFont[p_Header.NumFonts];
            for (uint l_I = 0; l_I < p_Header.NumFonts; ++l_I)
            {
                BaseStream.Position = p_Header.OffsetTable[l_I];
                l_Fonts[l_I] = new OpenTypeFont(this, p_LazyLoad);
            }

            return l_Fonts;
        }
    }
}
#endif

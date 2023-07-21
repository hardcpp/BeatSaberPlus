#if CP_SDK_UNITY
using System.IO;
using System.Text;

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType collection reader
    /// </summary>
    public class OpenTypeCollectionReader : OpenTypeFontReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Stream">Input stream</param>
        /// <param name="p_Encoding">Stream encoding</param>
        /// <param name="p_LeaveOpen">Should leave stream open?</param>
        public OpenTypeCollectionReader(Stream p_Stream, Encoding p_Encoding, bool p_LeaveOpen)
            : base(p_Stream, p_Encoding, p_LeaveOpen)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read collection header
        /// </summary>
        /// <returns></returns>
        public CollectionHeader ReadCollectionHeader()
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

        /// <summary>
        /// Read fonts
        /// </summary>
        /// <param name="p_Header">Fonts header</param>
        /// <param name="p_LazyLoad">Lazy loading</param>
        /// <returns></returns>
        public OpenTypeFont[] ReadFonts(CollectionHeader p_Header, bool p_LazyLoad = true)
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

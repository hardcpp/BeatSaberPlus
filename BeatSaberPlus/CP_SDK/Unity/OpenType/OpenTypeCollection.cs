#if CP_SDK_UNITY

namespace CP_SDK.Unity.OpenType
{
    /// <summary>
    /// OpenType collection
    /// </summary>
    public class OpenTypeCollection
    {
        private CollectionHeader            m_Header;
        private bool                        m_LazyLoad;
        private OpenTypeCollectionReader    m_Reader;
        private OpenTypeFont[]              m_Fonts;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Reader">Reader instance</param>
        /// <param name="p_LazyLoad">Lazy loading?</param>
        public OpenTypeCollection(OpenTypeCollectionReader p_Reader, bool p_LazyLoad = true)
            : this(p_Reader.ReadCollectionHeader(), p_Reader, p_LazyLoad)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Header">Collection header</param>
        /// <param name="p_Reader">Reader instance</param>
        /// <param name="p_LazyLoad">Lazy loading?</param>
        public OpenTypeCollection(CollectionHeader p_Header, OpenTypeCollectionReader p_Reader, bool p_LazyLoad = true)
        {
            m_Header    = p_Header;
            m_LazyLoad  = p_LazyLoad;
            m_Reader    = p_Reader;

            if (!p_LazyLoad)
                m_Fonts = m_Reader.ReadFonts(m_Header, m_LazyLoad);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get fonts
        /// </summary>
        /// <returns></returns>
        public OpenTypeFont[] GetFonts()
        {
            if (m_Fonts == null && m_LazyLoad)
                m_Fonts = m_Reader.ReadFonts(m_Header, m_LazyLoad);

            return m_Fonts;
        }
    }
}
#endif

using System.IO;
using System;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Animation Control chunk.
    /// </summary>
    internal class acTLChunk : Chunk
    {
        private uint m_FrameCount;
        private uint m_PlayCount;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number frames.
        /// </summary>
        /// <value>The number frames.</value>
        internal uint FrameCount => m_FrameCount;
        /// <summary>
        /// Gets the number plays.
        /// </summary>
        /// <value>The number plays.</value>
        internal uint PlayCount => m_PlayCount;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.acTLChunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk object.</param>
        internal acTLChunk(Chunk p_Chunk)
            : base(p_Chunk)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="p_MemoryStream">Memory stream to parse.</param>
        protected override void ParseData(MemoryStream p_MemoryStream)
        {
            m_FrameCount    = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
            m_PlayCount     = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
        }
    }
}
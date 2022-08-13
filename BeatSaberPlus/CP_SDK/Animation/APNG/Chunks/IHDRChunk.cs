using System;
using System.IO;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// PNG Image header chunk.
    /// </summary>
    internal class IHDRChunk : Chunk
    {
        private int m_Width;
        private int m_Height;
        private byte m_BitDepth;
        private byte m_ColorType;
        private byte m_CompressionMethod;
        private byte m_FilterMethod;
        private byte m_InterlaceMethod;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        internal int Width
        {
            get => m_Width;
            set
            {
                m_Width = value;
                ModifyChunkData(0, BitConverter.GetBytes(Helper.ConvertEndian(value)));
            }
        }
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        internal int Height
        {
            get => m_Height;
            set
            {
                m_Height = value;
                ModifyChunkData(4, BitConverter.GetBytes(Helper.ConvertEndian(value)));
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IHDRChunk"/> class.
        /// </summary>
        /// <param name="p_MemoryStream">Memory stream representation.</param>
        internal IHDRChunk(MemoryStream p_MemoryStream)
            : base(p_MemoryStream)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IHDRChunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk representation.</param>
        internal IHDRChunk(Chunk p_Chunk)
            : base(p_Chunk)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="p_MemoryStream">Memory stream of chunk data.</param>
        protected override void ParseData(MemoryStream p_MemoryStream)
        {
            m_Width             = Helper.ConvertEndian(p_MemoryStream.ReadInt32());
            m_Height            = Helper.ConvertEndian(p_MemoryStream.ReadInt32());
            m_BitDepth          = Convert.ToByte(p_MemoryStream.ReadByte());
            m_ColorType         = Convert.ToByte(p_MemoryStream.ReadByte());
            m_CompressionMethod = Convert.ToByte(p_MemoryStream.ReadByte());
            m_FilterMethod      = Convert.ToByte(p_MemoryStream.ReadByte());
            m_InterlaceMethod   = Convert.ToByte(p_MemoryStream.ReadByte());
        }
    }
}
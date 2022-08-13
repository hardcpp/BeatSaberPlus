using System;
using System.IO;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Animated PNG FDAT Chunk
    /// </summary>
    internal class fdATChunk : Chunk
    {
        private uint m_SequenceNumber;
        private byte[] m_FrameData;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sequence number of the animation chunk, starting from 0
        /// </summary>
        internal uint SequenceNumber => m_SequenceNumber;
        /// <summary>
        /// Gets or sets the frame data.
        /// </summary>
        /// <value>The frame data.</value>
        internal byte[] FrameData => m_FrameData;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.fdATChunk"/> class.
        /// </summary>
        /// <param name="p_MemoryStream">Memory stream of chunk data.</param>
        internal fdATChunk(MemoryStream p_MemoryStream)
            : base(p_MemoryStream)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.fdATChunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk data.</param>
        internal fdATChunk(Chunk p_Chunk)
            : base(p_Chunk)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts an FDAT Chunk to an IDAT Chunk.
        /// </summary>
        /// <returns>The IDAT chunk.</returns>
        internal IDATChunk ToIDATChunk()
        {
            uint l_NewCRC;
            using (var l_CRCStream = new MemoryStream())
            {
                l_CRCStream.WriteBytes(new[] { (byte)'I', (byte)'D', (byte)'A', (byte)'T' });
                l_CRCStream.WriteBytes(FrameData);

                l_NewCRC = Misc.CRC.Calculate(l_CRCStream.ToArray());
            }

            using (var l_DataStream = new MemoryStream())
            {
                l_DataStream.WriteUInt32(Helper.ConvertEndian(Length - 4));
                l_DataStream.WriteBytes(new[] { (byte)'I', (byte)'D', (byte)'A', (byte)'T' });
                l_DataStream.WriteBytes(FrameData);
                l_DataStream.WriteUInt32(Helper.ConvertEndian(l_NewCRC));
                l_DataStream.Position = 0;

                return new IDATChunk(l_DataStream);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="p_Stream">Memory Stream of chunk data.</param>
        protected override void ParseData(MemoryStream p_Stream)
        {
            m_SequenceNumber    = Helper.ConvertEndian(p_Stream.ReadUInt32());
            m_FrameData         = p_Stream.ReadBytes((int)Length - 4);
        }
    }
}
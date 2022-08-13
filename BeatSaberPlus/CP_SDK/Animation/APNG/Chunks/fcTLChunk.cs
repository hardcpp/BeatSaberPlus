using System.IO;
using System;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Enumeration of blend operations.
    /// </summary>
    internal enum EBlendOps
    {
        /// <summary>
        /// Do not blend use the source data.
        /// </summary>
        APNGBlendOpSource = 0,
        /// <summary>
        /// Perform composite blending.
        /// </summary>
        APNGBlendOpOver = 1,
    }
    /// <summary>
    /// Enumeration of dispose operations.
    /// </summary>
    internal enum EDisposeOps
    {
        /// <summary>
        /// Does not clear any of the previous drawing.
        /// </summary>
        APNGDisposeOpNone = 0,
        /// <summary>
        /// Clears the background to transparent black before rendering.
        /// </summary>
        APNGDisposeOpBackground = 1,
        /// <summary>
        /// Draws using the previous frame as the base.
        /// </summary>
        APNGDisposeOpPrevious = 2,
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Animated PNG Frame Control chunk.
    /// </summary>
    internal class fcTLChunk : Chunk
    {
        private uint m_SequenceNumber;
        private uint m_Width;
        private uint m_Height;
        private uint m_XOffset;
        private uint m_YOffset;
        private ushort m_DelayNumerator;
        private ushort m_DelayDenominator;
        private EDisposeOps m_DisposeOp;
        private EBlendOps m_BlendOp;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sequence number of the animation chunk, starting from 0
        /// </summary>
        internal uint SequenceNumber => m_SequenceNumber;
        /// <summary>
        /// Width of the following frame
        /// </summary>
        internal uint Width => m_Width;
        /// <summary>
        /// Height of the following frame
        /// </summary>
        internal uint Height => m_Height;
        /// <summary>
        /// X position at which to render the following frame
        /// </summary>
        internal uint XOffset => m_XOffset;
        /// <summary>
        /// Y position at which to render the following frame
        /// </summary>
        internal uint YOffset => m_YOffset;
        /// <summary>
        /// Frame delay fraction numerator
        /// </summary>
        internal ushort DelayNumerator => m_DelayNumerator;
        /// <summary>
        /// Frame delay fraction denominator
        /// </summary>
        internal ushort DelayDenominator => m_DelayDenominator;
        /// <summary>
        /// Type of frame area disposal to be done after rendering this frame
        /// </summary>
        internal EDisposeOps DisposeOp => m_DisposeOp;
        /// <summary>
        /// Type of frame area rendering for this frame
        /// </summary>
        internal EBlendOps BlendOp => m_BlendOp;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.fcTLChunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk data.</param>
        internal fcTLChunk(Chunk p_Chunk)
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
            m_SequenceNumber    = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
            m_Width             = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
            m_Height            = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
            m_XOffset           = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
            m_YOffset           = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
            m_DelayNumerator    = Helper.ConvertEndian(p_MemoryStream.ReadUInt16());
            m_DelayDenominator  = Helper.ConvertEndian(p_MemoryStream.ReadUInt16());
            m_DisposeOp         = (EDisposeOps)p_MemoryStream.ReadByte();
            m_BlendOp           = (EBlendOps)p_MemoryStream.ReadByte();
        }
    }
}
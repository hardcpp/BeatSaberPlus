using System.IO;
using System;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Enumeration of dispose operations.
    /// </summary>
    public enum DisposeOps
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

    /// <summary>
    /// Enumeration of blend operations.
    /// </summary>
    public enum BlendOps
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
    /// Animated PNG Frame Control chunk.
    /// </summary>
    internal class fcTLChunk : Chunk
    {
        private uint sequenceNumber;
        private uint width;
        private uint height;
        private uint xOffset;
        private uint yOffset;
        private ushort delayNumerator;
        private ushort delayDenominator;
        private DisposeOps disposeOp;
        private BlendOps blendOp;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.fcTLChunk"/> class.
        /// </summary>
        internal fcTLChunk()
        {
            Length = 26;
            ChunkType = "fcTL";
            ChunkData = new byte[Length];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.fcTLChunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk data.</param>
        public fcTLChunk(Chunk chunk)
            : base(chunk)
        {
        }

        /// <summary>
        /// Sequence number of the animation chunk, starting from 0
        /// </summary>
        public uint SequenceNumber
        {
            get
            {
                return sequenceNumber;
            }
            internal set
            {
                sequenceNumber = value;
                ModifyChunkData(0, Helper.ConvertEndian(value));
            }
        }

        /// <summary>
        /// Width of the following frame
        /// </summary>
        public uint Width
        {
            get
            {
                return width;
            }
            internal set
            {
                width = value;
                ModifyChunkData(4, Helper.ConvertEndian(value));
            }
        }

        /// <summary>
        /// Height of the following frame
        /// </summary>
        public uint Height
        {
            get
            {
                return height;
            }
            internal set
            {
                height = value;
                ModifyChunkData(8, Helper.ConvertEndian(value));
            }
        }

        /// <summary>
        /// X position at which to render the following frame
        /// </summary>
        public uint XOffset
        {
            get
            {
                return xOffset;
            }
            internal set
            {
                xOffset = value;
                ModifyChunkData(12, Helper.ConvertEndian(value));
            }
        }

        /// <summary>
        /// Y position at which to render the following frame
        /// </summary>
        public uint YOffset
        {
            get
            {
                return yOffset;
            }
            internal set
            {
                yOffset = value;
                ModifyChunkData(16, Helper.ConvertEndian(value));
            }
        }

        /// <summary>
        /// Frame delay fraction numerator
        /// </summary>
        public ushort DelayNumerator
        {
            get
            {
                return delayNumerator;
            }
            internal set
            {
                delayNumerator = value;
                ModifyChunkData(20, BitConverter.GetBytes(Helper.ConvertEndian(value)));
            }
        }

        /// <summary>
        /// Frame delay fraction denominator
        /// </summary>
        public ushort DelayDenominator
        {
            get
            {
                return delayDenominator;
            }
            internal set
            {
                delayDenominator = value;
                ModifyChunkData(22, BitConverter.GetBytes(Helper.ConvertEndian(value)));
            }
        }

        /// <summary>
        /// Type of frame area disposal to be done after rendering this frame
        /// </summary>
        public DisposeOps DisposeOp
        {
            get
            {
                return disposeOp;
            }
            internal set
            {
                disposeOp = value;
                ModifyChunkData(24, new byte[]{ (byte)value });
            }
        }

        /// <summary>
        /// Type of frame area rendering for this frame
        /// </summary>
        public BlendOps BlendOp
        {
            get
            {
                return blendOp;
            }
            internal set
            {
                blendOp = value;
                ModifyChunkData(25, new byte[]{ (byte)value });
            }
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="ms">Memory stream of chunk data.</param>
        protected override void ParseData(MemoryStream ms)
        {
            sequenceNumber = Helper.ConvertEndian(ms.ReadUInt32());
            width = Helper.ConvertEndian(ms.ReadUInt32());
            height = Helper.ConvertEndian(ms.ReadUInt32());
            xOffset = Helper.ConvertEndian(ms.ReadUInt32());
            yOffset = Helper.ConvertEndian(ms.ReadUInt32());
            delayNumerator = Helper.ConvertEndian(ms.ReadUInt16());
            delayDenominator = Helper.ConvertEndian(ms.ReadUInt16());
            disposeOp = (DisposeOps)ms.ReadByte();
            blendOp = (BlendOps)ms.ReadByte();
        }
    }
}
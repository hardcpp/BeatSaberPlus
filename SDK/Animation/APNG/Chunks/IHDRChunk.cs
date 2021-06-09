using System;
using System.IO;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// PNG Image header chunk.
    /// </summary>
    internal class IHDRChunk : Chunk
    {
        private int width;
        private int height;
        private byte bitDepth;
        private byte colorType;
        private byte compressionMethod;
        private byte filterMethod;
        private byte interlaceMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IHDRChunk"/> class.
        /// </summary>
        /// <param name="ms">Memory stream representation.</param>
        public IHDRChunk(MemoryStream ms)
            : base(ms)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IHDRChunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk representation.</param>
        public IHDRChunk(Chunk chunk)
            : base(chunk)
        {
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width
        {
            get
            {
                return this.width;
            }
            internal set
            {
                this.width = value;
                ModifyChunkData(0, BitConverter.GetBytes(Helper.ConvertEndian(value)));
            }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height
        {
            get
            {
                return this.height;
            }
            internal set
            {
                this.height = value;
                ModifyChunkData(4, BitConverter.GetBytes(Helper.ConvertEndian(value)));
            }
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="ms">Memory stream of chunk data.</param>
        protected override void ParseData(MemoryStream ms)
        {
            width = Helper.ConvertEndian(ms.ReadInt32());
            height = Helper.ConvertEndian(ms.ReadInt32());
            bitDepth = Convert.ToByte(ms.ReadByte());
            colorType = Convert.ToByte(ms.ReadByte());
            compressionMethod = Convert.ToByte(ms.ReadByte());
            filterMethod = Convert.ToByte(ms.ReadByte());
            interlaceMethod = Convert.ToByte(ms.ReadByte());
        }
    }
}
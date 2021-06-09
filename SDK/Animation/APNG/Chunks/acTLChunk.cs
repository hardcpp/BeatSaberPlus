using System.IO;
using System;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Animation Control chunk.
    /// </summary>
    internal class acTLChunk : Chunk
    {
        private uint frameCount;
        private uint playCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.acTLChunk"/> class.
        /// </summary>
        internal acTLChunk()
        {
            Length = 8;
            ChunkType = "acTL";
            ChunkData = new byte[Length];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.acTLChunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk object.</param>
        public acTLChunk(Chunk chunk)
            : base(chunk)
        {
        }

        /// <summary>
        /// Gets the number frames.
        /// </summary>
        /// <value>The number frames.</value>
        public uint FrameCount
        {
            get
            {
                return frameCount;
            }
            internal set
            {
                frameCount = value;
                ModifyChunkData(0, Helper.ConvertEndian(value));
            }
        }

        /// <summary>
        /// Gets the number plays.
        /// </summary>
        /// <value>The number plays.</value>
        public uint PlayCount
        {
            get
            {
                return playCount;
            }
            internal set
            {
                playCount = value;
                ModifyChunkData(4, Helper.ConvertEndian(value));
            }
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="ms">Memory stream to parse.</param>
        protected override void ParseData(MemoryStream ms)
        {
            frameCount = Helper.ConvertEndian(ms.ReadUInt32());
            playCount = Helper.ConvertEndian(ms.ReadUInt32());
        }
    }
}
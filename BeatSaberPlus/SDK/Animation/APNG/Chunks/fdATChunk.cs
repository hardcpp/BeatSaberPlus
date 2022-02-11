using System;
using System.IO;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Animated PNG FDAT Chunk
    /// </summary>
    internal class fdATChunk : Chunk
    {
        private uint sequenceNumber;
        private byte[] frameData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.fdATChunk"/> class.
        /// </summary>
        /// <param name="ms">Memory stream of chunk data.</param>
        public fdATChunk(MemoryStream ms)
            : base(ms)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.fdATChunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk data.</param>
        public fdATChunk(Chunk chunk)
            : base(chunk)
        {
        }

        /// <summary>
        /// Gets or sets the frame data.
        /// </summary>
        /// <value>The frame data.</value>
        public byte[] FrameData
        {
            get
            {
                return this.frameData;
            }
            internal set
            {
                this.frameData = value;
                ModifyChunkData(4, value);
            }
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="p_Stream">Memory Stream of chunk data.</param>
        protected override void ParseData(MemoryStream p_Stream)
        {
            sequenceNumber = Helper.ConvertEndian(p_Stream.ReadUInt32());
            frameData = p_Stream.ReadBytes((int)Length - 4);
        }

        /// <summary>
        /// Converts an FDAT Chunk to an IDAT Chunk.
        /// </summary>
        /// <returns>The IDAT chunk.</returns>
        public IDATChunk ToIDATChunk()
        {
            uint l_NewCRC;
            using (var l_CRCStream = new MemoryStream())
            {
                l_CRCStream.WriteBytes(new[] {(byte)'I', (byte)'D', (byte)'A', (byte)'T'});
                l_CRCStream.WriteBytes(FrameData);

                l_NewCRC = Misc.CRC.Calculate(l_CRCStream.ToArray());
            }

            using (var l_DataStream = new MemoryStream())
            {
                l_DataStream.WriteUInt32(Helper.ConvertEndian(Length - 4));
                l_DataStream.WriteBytes(new[] {(byte)'I', (byte)'D', (byte)'A', (byte)'T'});
                l_DataStream.WriteBytes(FrameData);
                l_DataStream.WriteUInt32(Helper.ConvertEndian(l_NewCRC));
                l_DataStream.Position = 0;

                return new IDATChunk(l_DataStream);
            }
        }
    }
}
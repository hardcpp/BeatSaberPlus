using System;
using System.IO;
using System.Text;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Base PNG Chunk object.
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.Chunk"/> class.
        /// </summary>
        internal Chunk()
        {
            Length = 0;
            ChunkType = String.Empty;
            ChunkData = null;
            Crc = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.Chunk"/> class.
        /// </summary>
        /// <param name="bytes">Byte Array of chiunk data.</param>
        internal Chunk(byte[] bytes)
        {
            var ms = new MemoryStream(bytes);
            Length = Helper.ConvertEndian(ms.ReadUInt32());
            ChunkType = Encoding.ASCII.GetString(ms.ReadBytes(4));
            ChunkData = ms.ReadBytes((int)Length);
            Crc = Helper.ConvertEndian(ms.ReadUInt32());

            if (ms.Position != ms.Length)
                throw new Exception("Chunk length not correct.");
            if (Length != ChunkData.Length)
                throw new Exception("Chunk data length not correct.");

            ParseData(new MemoryStream(ChunkData));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.Chunk"/> class.
        /// </summary>
        /// <param name="ms">Memory Stream of chunk data.</param>
        internal Chunk(MemoryStream ms)
        {
            Length = Helper.ConvertEndian(ms.ReadUInt32());
            ChunkType = Encoding.ASCII.GetString(ms.ReadBytes(4));
            ChunkData = ms.ReadBytes((int)Length);
            Crc = Helper.ConvertEndian(ms.ReadUInt32());

            ParseData(new MemoryStream(ChunkData));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.Chunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk data.</param>
        internal Chunk(Chunk chunk)
        {
            Length = chunk.Length;
            ChunkType = chunk.ChunkType;
            ChunkData = chunk.ChunkData;
            Crc = chunk.Crc;

            ParseData(new MemoryStream(ChunkData));
        }

        /// <summary>
        /// Gets or sets the length of the chunk data.
        /// </summary>
        /// <value>The length of the chunk data.</value>
        public uint Length { get; set; }

        /// <summary>
        /// Gets or sets the type of the chunk.
        /// </summary>
        /// <value>The type of the chunk.</value>
        public string ChunkType { get; set; }

        /// <summary>
        /// Gets or sets the chunk data.
        /// </summary>
        /// <value>The chunk data.</value>
        public byte[] ChunkData { get; set; }

        /// <summary>
        /// Gets or sets the crc.
        /// </summary>
        /// <value>The crc.</value>
        public uint Crc { get; set; }

        /// <summary>
        /// Get raw data of the chunk
        /// </summary>
        public byte[] RawData
        {
            get
            {
                var ms = new MemoryStream();
                ms.WriteUInt32(Helper.ConvertEndian(Length));
                ms.WriteBytes(Encoding.ASCII.GetBytes(ChunkType));
                ms.WriteBytes(ChunkData);
                ms.WriteUInt32(Helper.ConvertEndian(Crc));

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Modify the ChunkData part.
        /// </summary>
        public void ModifyChunkData(int postion, byte[] newData)
        {
            Array.Copy(newData, 0, ChunkData, postion, newData.Length);

            using (var msCrc = new MemoryStream())
            {
                msCrc.WriteBytes(Encoding.ASCII.GetBytes(ChunkType));
                msCrc.WriteBytes(ChunkData);

                Crc = Misc.CRC.Calculate(msCrc.ToArray());
            }
        }

        /// <summary>
        /// Modify the ChunkData part.
        /// </summary>
        public void ModifyChunkData(int postion, uint newData)
        {
            ModifyChunkData(postion, BitConverter.GetBytes(newData));
        }

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="ms">Memory Stream of chunk data.</param>
        protected virtual void ParseData(MemoryStream ms)
        {
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="AnimatedImages.Chunk"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="AnimatedImages.Chunk"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="AnimatedImages.Chunk"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj == null)
            {
                result = false;
            }
            else
            {
                Chunk chunk = obj as Chunk;
                if (chunk != null)
                {
                    result = (this.Length == chunk.Length &&
                    this.ChunkType == chunk.ChunkType &&
                    this.Crc == chunk.Crc);
                }
            }

            return result;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="AnimatedImages.Chunk"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            //TODO: Build a better hash code. Perhaps for equality where chunktype bytes XOR'd with crc.
            return base.GetHashCode();
        }
    }
}
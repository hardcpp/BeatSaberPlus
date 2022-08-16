using System;
using System.IO;
using System.Text;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Base PNG Chunk object.
    /// </summary>
    internal class Chunk
    {
        /// <summary>
        /// Gets or sets the length of the chunk data.
        /// </summary>
        /// <value>The length of the chunk data.</value>
        internal uint Length { get; set; }
        /// <summary>
        /// Gets or sets the type of the chunk.
        /// </summary>
        /// <value>The type of the chunk.</value>
        internal string ChunkType { get; set; }
        /// <summary>
        /// Gets or sets the chunk data.
        /// </summary>
        /// <value>The chunk data.</value>
        internal byte[] ChunkData { get; set; }
        /// <summary>
        /// Gets or sets the crc.
        /// </summary>
        /// <value>The crc.</value>
        internal uint Crc { get; set; }
        /// <summary>
        /// Get raw data of the chunk
        /// </summary>
        internal byte[] RawData
        {
            get
            {
                var l_MemoryStream = new MemoryStream();
                l_MemoryStream.WriteUInt32(Helper.ConvertEndian(Length));
                l_MemoryStream.WriteBytes(Encoding.ASCII.GetBytes(ChunkType));
                l_MemoryStream.WriteBytes(ChunkData);
                l_MemoryStream.WriteUInt32(Helper.ConvertEndian(Crc));

                return l_MemoryStream.ToArray();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.Chunk"/> class.
        /// </summary>
        /// <param name="p_MemoryStream">Memory Stream of chunk data.</param>
        internal Chunk(MemoryStream p_MemoryStream)
        {
            Length      = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());
            ChunkType   = Encoding.ASCII.GetString(p_MemoryStream.ReadBytes(4));
            ChunkData   = p_MemoryStream.ReadBytes((int)Length);
            Crc         = Helper.ConvertEndian(p_MemoryStream.ReadUInt32());

            ParseData(new MemoryStream(ChunkData));
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.Chunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk data.</param>
        internal Chunk(Chunk p_Chunk)
        {
            Length      = p_Chunk.Length;
            ChunkType   = p_Chunk.ChunkType;
            ChunkData   = p_Chunk.ChunkData;
            Crc         = p_Chunk.Crc;

            ParseData(new MemoryStream(ChunkData));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Modify the ChunkData part.
        /// </summary>
        internal void ModifyChunkData(int p_Position, byte[] p_NewData)
        {
            Array.Copy(p_NewData, 0, ChunkData, p_Position, p_NewData.Length);

            using (var l_MemoryStream = new MemoryStream())
            {
                l_MemoryStream.WriteBytes(Encoding.ASCII.GetBytes(ChunkType));
                l_MemoryStream.WriteBytes(ChunkData);

                Crc = Misc.CRC.Calculate(l_MemoryStream.ToArray());
            }
        }
        /// <summary>
        /// Modify the ChunkData part.
        /// </summary>
        internal void ModifyChunkData(int p_Position, uint p_NewData)
            => ModifyChunkData(p_Position, BitConverter.GetBytes(p_NewData));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="p_MemoryStream">Memory Stream of chunk data.</param>
        protected virtual void ParseData(MemoryStream p_MemoryStream)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="AnimatedImages.Chunk"/>.
        /// </summary>
        /// <param name="p_Other">The <see cref="System.Object"/> to compare with the current <see cref="AnimatedImages.Chunk"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="AnimatedImages.Chunk"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object p_Other)
        {
            var l_Result = false;
            if (p_Other == null)
                l_Result = false;
            else if (p_Other is Chunk l_Chunk && l_Chunk != null)
                l_Result = (Length == l_Chunk.Length && ChunkType == l_Chunk.ChunkType && Crc == l_Chunk.Crc);

            return l_Result;
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
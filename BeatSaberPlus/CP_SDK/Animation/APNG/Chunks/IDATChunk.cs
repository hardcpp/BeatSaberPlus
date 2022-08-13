using System.IO;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Standard IDAT PNG image data chunk.
    /// </summary>
    internal class IDATChunk : Chunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IDATChunk"/> class.
        /// </summary>
        /// <param name="p_MemoryStream">Memory stream representation..</param>
        internal IDATChunk(MemoryStream p_MemoryStream)
            : base(p_MemoryStream)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IDATChunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk representation.</param>
        internal IDATChunk(Chunk p_Chunk)
            : base(p_Chunk)
        {

        }
    }
}
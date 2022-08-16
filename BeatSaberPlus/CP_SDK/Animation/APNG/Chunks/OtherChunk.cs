using System.IO;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Other PNG chunks.
    /// </summary>
    internal class OtherChunk : Chunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.OtherChunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk data.</param>
        internal OtherChunk(Chunk p_Chunk)
            : base(p_Chunk)
        {

        }
    }
}
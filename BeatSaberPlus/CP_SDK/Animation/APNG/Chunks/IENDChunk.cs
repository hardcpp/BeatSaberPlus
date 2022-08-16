using System.IO;

namespace CP_SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// IEND chunk representing the end of the PNG
    /// </summary>
    internal class IENDChunk : Chunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IENDChunk"/> class.
        /// </summary>
        /// <param name="p_Chunk">Chunk representation.</param>
        internal IENDChunk(Chunk p_Chunk)
            : base(p_Chunk)
        {

        }
    }
}
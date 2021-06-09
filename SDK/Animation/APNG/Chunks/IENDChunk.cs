using System.IO;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// IEND chunk representing the end of the PNG
    /// </summary>
    internal class IENDChunk : Chunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IENDChunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk representation.</param>
        public IENDChunk(Chunk chunk)
            : base(chunk)
        {
        }
    }
}
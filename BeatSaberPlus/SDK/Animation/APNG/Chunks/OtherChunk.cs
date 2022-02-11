using System.IO;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Other PNG chunks.
    /// </summary>
    internal class OtherChunk : Chunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.OtherChunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk data.</param>
        public OtherChunk(Chunk chunk)
            : base(chunk)
        {

        }
    }
}
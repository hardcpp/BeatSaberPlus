using System.IO;

namespace BeatSaberPlus.SDK.Animation.APNG.Chunks
{
    /// <summary>
    /// Standard IDAT PNG image data chunk.
    /// </summary>
    internal class IDATChunk : Chunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IDATChunk"/> class.
        /// </summary>
        /// <param name="ms">Memory stream representation..</param>
        public IDATChunk(MemoryStream ms)
            : base(ms)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedImages.IDATChunk"/> class.
        /// </summary>
        /// <param name="chunk">Chunk representation.</param>
        public IDATChunk(Chunk chunk)
            : base(chunk)
        {

        }
    }
}
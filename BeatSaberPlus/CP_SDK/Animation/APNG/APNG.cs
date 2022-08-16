using CP_SDK.Animation.APNG.Chunks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CP_SDK.Animation.APNG
{
    /// <summary>
    /// Animated PNG class.
    /// </summary>
    public class APNG
    {
        private Frame m_DefaultImage = new Frame();
        private List<Frame> m_Frames = new List<Frame>();
        private Size viewSize;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Gets the IHDR Chunk
        /// </summary>
        internal IHDRChunk IHDRChunk { get; private set; }
        /// <summary>
        ///     Gets the acTL Chunk
        /// </summary>
        internal acTLChunk acTLChunk { get; private set; }
        /// <summary>
        ///     Indicate whether the file is a simple PNG.
        /// </summary>
        internal bool IsSimplePNG { get; private set; }
        /// <summary>
        ///     Indicate whether the default image is part of the animation
        /// </summary>
        internal bool DefaultImageIsAnimated { get; private set; }

        /// <summary>
        ///     Gets the base image.
        ///     If IsSimplePNG = True, returns the only image;
        ///     if False, returns the default image
        /// </summary>
        internal Frame DefaultImage => m_DefaultImage;
        /// <summary>
        ///     Gets the frame array.
        ///     If IsSimplePNG = True, returns empty
        /// </summary>
        internal Frame[] Frames => IsSimplePNG ? new Frame[] { m_DefaultImage } : m_Frames.ToArray();
        /// <summary>
        /// Gets the frame count.
        /// </summary>
        internal int FrameCount => (int)(acTLChunk?.FrameCount ?? 1);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates an Animated PNG from a stream.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="p_Stream">The stream.</param>
        public static APNG FromStream(MemoryStream p_Stream)
        {
            APNG l_Result = new APNG();
            l_Result.Load(p_Stream);

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load the specified stream.
        /// </summary>
        /// <param name="p_MemoryStream">Streamrepresentation of the png file.</param>
        internal void Load(MemoryStream p_MemoryStream)
        {
            if (!Helper.IsBytesEqual(p_MemoryStream.ReadBytes(Frame.Signature.Length), Frame.Signature))
                throw new Exception("File signature incorrect.");

            IHDRChunk = new IHDRChunk(p_MemoryStream);
            if (IHDRChunk.ChunkType != "IHDR")
                throw new Exception("IHDR chunk must located before any other chunks.");

            viewSize = new Size(IHDRChunk.Width, IHDRChunk.Height);

            var l_Chunk                 = null as Chunk;
            var l_Frame                 = null as Frame;
            var l_OtherChunks           = new List<OtherChunk>();
            var l_IsIDATAlreadyParsed   = false;

            do
            {
                if (p_MemoryStream.Position == p_MemoryStream.Length)
                    throw new Exception("IEND chunk expected.");

                l_Chunk = new Chunk(p_MemoryStream);

                switch (l_Chunk.ChunkType)
                {
                    case "IHDR":
                        throw new Exception("Only single IHDR is allowed.");

                    case "acTL":
                        if (IsSimplePNG)
                            throw new Exception("acTL chunk must located before any IDAT and fdAT");

                        acTLChunk = new acTLChunk(l_Chunk);
                        break;

                    case "IDAT":
                        if (acTLChunk == null)
                            IsSimplePNG = true;

                        m_DefaultImage.IHDRChunk = IHDRChunk;
                        m_DefaultImage.AddIDATChunk(new IDATChunk(l_Chunk));
                        l_IsIDATAlreadyParsed = true;
                        break;

                    case "fcTL":
                        if (IsSimplePNG)
                            continue;

                        if (l_Frame != null && l_Frame.IDATChunks.Count == 0)
                            throw new Exception("One frame must have only one fcTL chunk.");

                        if (l_IsIDATAlreadyParsed)
                        {
                            if (l_Frame != null)
                                m_Frames.Add(l_Frame);

                            l_Frame = new Frame
                            {
                                IHDRChunk = IHDRChunk,
                                fcTLChunk = new fcTLChunk(l_Chunk)
                            };
                        }
                        else
                            m_DefaultImage.fcTLChunk = new fcTLChunk(l_Chunk);

                        break;

                    case "fdAT":
                        if (IsSimplePNG)
                            continue;

                        if (l_Frame == null || l_Frame.fcTLChunk == null)
                            throw new Exception("fcTL chunk expected.");

                        l_Frame.AddIDATChunk(new fdATChunk(l_Chunk).ToIDATChunk());
                        break;

                    case "IEND":
                        if (l_Frame != null)
                            m_Frames.Add(l_Frame);

                        if (DefaultImage.IDATChunks.Count != 0)
                            DefaultImage.IENDChunk = new IENDChunk(l_Chunk);

                        foreach (Frame f in m_Frames)
                        {
                            f.IENDChunk = new IENDChunk(l_Chunk);
                        }

                        break;

                    default:
                        l_OtherChunks.Add(new OtherChunk(l_Chunk));
                        break;

                }
            } while (l_Chunk.ChunkType != "IEND");


            if (m_DefaultImage.fcTLChunk != null)
            {
                m_Frames.Insert(0, m_DefaultImage);
                DefaultImageIsAnimated = true;
            }
            else
                l_OtherChunks.ForEach(m_DefaultImage.AddOtherChunk);

            m_Frames.ForEach(x => l_OtherChunks.ForEach(x.AddOtherChunk));
        }
    }
}
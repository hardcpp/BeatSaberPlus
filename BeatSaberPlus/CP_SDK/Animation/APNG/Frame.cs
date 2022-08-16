using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;
using CP_SDK.Animation.APNG.Chunks;

namespace CP_SDK.Animation.APNG
{
    /// <summary>
    ///     Describe a single frame.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// The chunk signature.
        /// </summary>
        public static byte[] Signature = {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<IDATChunk> m_IDATChunks = new List<IDATChunk>();
        private List<OtherChunk> m_OtherChunks = new List<OtherChunk>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Gets or Sets the acTL chunk
        /// </summary>
        internal IHDRChunk IHDRChunk { get; set; }
        /// <summary>
        ///     Gets or Sets the fcTL chunk
        /// </summary>
        internal fcTLChunk fcTLChunk { get; set; }
        /// <summary>
        ///     Gets or Sets the IEND chunk
        /// </summary>
        internal IENDChunk IENDChunk { get; set; }

        /// <summary>
        ///     Gets or Sets the IDAT chunks
        /// </summary>
        internal List<IDATChunk> IDATChunks => m_IDATChunks;

        /// <summary>
        /// Gets or sets the frame rate.
        /// </summary>
        /// <value>The frame rate in milliseconds.</value>
        /// <remarks>Should not be less than 10 ms or animation will not occur.</remarks>
        internal int FrameRate
        {
            get
            {
                int     l_FrameRate         = fcTLChunk.DelayNumerator;
                double  l_DenominatorOffset = 1000 / fcTLChunk.DelayDenominator;

                if ((int)Math.Round(l_DenominatorOffset) != 1)
                    l_FrameRate = (int)(fcTLChunk.DelayNumerator * l_DenominatorOffset);

                return l_FrameRate;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts the Frame to a Bitmap
        /// </summary>
        /// <returns>The bitmap of the frame.</returns>
        internal Bitmap ToBitmap()
        {
            var l_BaseBitmap    = (Bitmap)Image.FromStream(GetStream());
            var l_FinalBitmap   = new Bitmap(IHDRChunk.Width, IHDRChunk.Height);

            var l_Graphics = Graphics.FromImage(l_FinalBitmap);
            l_Graphics.CompositingMode      = CompositingMode.SourceOver;
            l_Graphics.CompositingQuality   = CompositingQuality.GammaCorrected;
            l_Graphics.Clear(Color.FromArgb(0x00000000));
            l_Graphics.DrawImage(l_BaseBitmap, fcTLChunk.XOffset, fcTLChunk.YOffset, fcTLChunk.Width, fcTLChunk.Height);

            return l_FinalBitmap;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Add an Chunk to end end of existing list.
        /// </summary>
        internal void AddOtherChunk(OtherChunk p_Chunk)
            => m_OtherChunks.Add(p_Chunk);
        /// <summary>
        ///     Add an IDAT Chunk to end end of existing list.
        /// </summary>
        internal void AddIDATChunk(IDATChunk p_Chunk)
            => m_IDATChunks.Add(p_Chunk);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Gets the frame as PNG FileStream.
        /// </summary>
        internal MemoryStream GetStream()
        {
            var l_IHDRChunk = new IHDRChunk(IHDRChunk);
            if (fcTLChunk != null)
            {
                l_IHDRChunk.ModifyChunkData(0, Helper.ConvertEndian(fcTLChunk.Width));
                l_IHDRChunk.ModifyChunkData(4, Helper.ConvertEndian(fcTLChunk.Height));
            }

            var l_MemoryStream = new MemoryStream();
            l_MemoryStream.WriteBytes(Signature);
            l_MemoryStream.WriteBytes(l_IHDRChunk.RawData);
            m_OtherChunks.ForEach(x => l_MemoryStream.WriteBytes(x.RawData));
            m_IDATChunks.ForEach(x => l_MemoryStream.WriteBytes(x.RawData));
            l_MemoryStream.WriteBytes(IENDChunk.RawData);

            l_MemoryStream.Position = 0;

            return l_MemoryStream;
        }
    }
}
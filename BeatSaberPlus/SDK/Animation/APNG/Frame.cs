using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;
using BeatSaberPlus.SDK.Animation.APNG.Chunks;

namespace BeatSaberPlus.SDK.Animation
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

        private List<IDATChunk> idatChunks = new List<IDATChunk>();
        private List<OtherChunk> otherChunks = new List<OtherChunk>();

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
        internal List<IDATChunk> IDATChunks
        {
            get { return idatChunks; }
            set { idatChunks = value; }
        }

        /// <summary>
        ///     Add an Chunk to end end of existing list.
        /// </summary>
        internal void AddOtherChunk(OtherChunk chunk)
        {
            otherChunks.Add(chunk);
        }

        /// <summary>
        ///     Add an IDAT Chunk to end end of existing list.
        /// </summary>
        internal void AddIDATChunk(IDATChunk chunk)
        {
            idatChunks.Add(chunk);
        }

        /// <summary>
        ///     Gets the frame as PNG FileStream.
        /// </summary>
        public MemoryStream GetStream()
        {
            var ihdrChunk = new IHDRChunk(IHDRChunk);
            if (fcTLChunk != null)
            {
                // Fix frame size with fcTL data.
                ihdrChunk.ModifyChunkData(0, Helper.ConvertEndian(fcTLChunk.Width));
                ihdrChunk.ModifyChunkData(4, Helper.ConvertEndian(fcTLChunk.Height));
            }

            // Write image data
            var ms = new MemoryStream();
            ms.WriteBytes(Signature);
            ms.WriteBytes(ihdrChunk.RawData);
            otherChunks.ForEach(o => ms.WriteBytes(o.RawData));
            idatChunks.ForEach(i => ms.WriteBytes(i.RawData));
            ms.WriteBytes(IENDChunk.RawData);

            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Converts the Frame to a Bitmap
        /// </summary>
        /// <returns>The bitmap of the frame.</returns>
        public Bitmap ToBitmap()
        {
            // Create the bitmap
            Bitmap b = (Bitmap)Image.FromStream(GetStream());

            Bitmap final = new Bitmap(IHDRChunk.Width, IHDRChunk.Height);

            Graphics g = Graphics.FromImage(final);
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.GammaCorrected;
            g.Clear(Color.FromArgb(0x00000000));
            g.DrawImage(b, fcTLChunk.XOffset, fcTLChunk.YOffset, fcTLChunk.Width, fcTLChunk.Height);

            return final;
        }

        /// <summary>
        /// Gets or sets the frame rate.
        /// </summary>
        /// <value>The frame rate in milliseconds.</value>
        /// <remarks>Should not be less than 10 ms or animation will not occur.</remarks>
        public int FrameRate
        {
            get
            {
                int frameRate = fcTLChunk.DelayNumerator;
                double denominatorOffset = 1000 / fcTLChunk.DelayDenominator;
                if((int)Math.Round(denominatorOffset) != 1) //If not millisecond based make it so for easier processing
                {
                    frameRate = (int)(fcTLChunk.DelayNumerator * denominatorOffset);
                }

                return frameRate;
            }
            internal set
            {
                //Standardize to milliseconds.

                fcTLChunk.DelayNumerator = (ushort)(value);
                fcTLChunk.DelayDenominator = 1000;
            }
        }
    }
}
using BeatSaberPlus.SDK.Animation.APNG.Chunks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BeatSaberPlus.SDK.Animation.APNG
{
    /// <summary>
    /// Animated PNG class.
    /// </summary>
    public class APNG
    {
        private Frame defaultImage = new Frame();
        private List<Frame> frames = new List<Frame>();
        private MemoryStream ms;
        private Size viewSize;

        /// <summary>
        /// Load the specified png.
        /// </summary>
        /// <param name="filename">The png filename.</param>
        public void Load(string filename)
        {
            Load(File.ReadAllBytes(filename));
        }

        /// <summary>
        /// Load the specified png.
        /// </summary>
        /// <param name="fileBytes">Byte representation of the png file.</param>
        public void Load(byte[] fileBytes)
        {
            MemoryStream stream = new MemoryStream(fileBytes);

            Load(stream);
        }

        /// <summary>
        /// Load the specified stream.
        /// </summary>
        /// <param name="stream">Streamrepresentation of the png file.</param>
        internal void Load(MemoryStream stream)
        {
            ms = stream;

            // check file signature.
            if (!Helper.IsBytesEqual(ms.ReadBytes(Frame.Signature.Length), Frame.Signature))
                throw new Exception("File signature incorrect.");

            // Read IHDR chunk.
            IHDRChunk = new IHDRChunk(ms);
            if (IHDRChunk.ChunkType != "IHDR")
                throw new Exception("IHDR chunk must located before any other chunks.");

            viewSize = new Size(IHDRChunk.Width, IHDRChunk.Height);

            // Now let's loop in chunks
            Chunk chunk;
            Frame frame = null;
            var otherChunks = new List<OtherChunk>();
            bool isIDATAlreadyParsed = false;
            do
            {
                if (ms.Position == ms.Length)
                    throw new Exception("IEND chunk expected.");

                chunk = new Chunk(ms);

                switch (chunk.ChunkType)
                {
                    case "IHDR":
                        throw new Exception("Only single IHDR is allowed.");

                    case "acTL":
                        if (IsSimplePNG)
                            throw new Exception("acTL chunk must located before any IDAT and fdAT");

                        acTLChunk = new acTLChunk(chunk);
                        break;

                    case "IDAT":
                        // To be an APNG, acTL must located before any IDAT and fdAT.
                        if (acTLChunk == null)
                            IsSimplePNG = true;

                        // Only default image has IDAT.
                        defaultImage.IHDRChunk = IHDRChunk;
                        defaultImage.AddIDATChunk(new IDATChunk(chunk));
                        isIDATAlreadyParsed = true;
                        break;

                    case "fcTL":
                        // Simple PNG should ignore this.
                        if (IsSimplePNG)
                            continue;

                        if (frame != null && frame.IDATChunks.Count == 0)
                            throw new Exception("One frame must have only one fcTL chunk.");

                        // IDAT already parsed means this fcTL is used by FRAME IMAGE.
                        if (isIDATAlreadyParsed)
                        {
                            // register current frame object and build a new frame object
                            // for next use
                            if (frame != null)
                                frames.Add(frame);
                            frame = new Frame
                                {
                                    IHDRChunk = IHDRChunk,
                                    fcTLChunk = new fcTLChunk(chunk)
                                };
                        }
                        // Otherwise this fcTL is used by the DEFAULT IMAGE.
                        else
                        {
                            defaultImage.fcTLChunk = new fcTLChunk(chunk);
                        }
                        break;
                    case "fdAT":
                        // Simple PNG should ignore this.
                        if (IsSimplePNG)
                            continue;

                        // fdAT is only used by frame image.
                        if (frame == null || frame.fcTLChunk == null)
                            throw new Exception("fcTL chunk expected.");

                        frame.AddIDATChunk(new fdATChunk(chunk).ToIDATChunk());
                        break;

                    case "IEND":
                        // register last frame object
                        if (frame != null)
                            frames.Add(frame);

                        if (DefaultImage.IDATChunks.Count != 0)
                            DefaultImage.IENDChunk = new IENDChunk(chunk);
                        foreach (Frame f in frames)
                        {
                            f.IENDChunk = new IENDChunk(chunk);
                        }
                        break;

                    default:
                        otherChunks.Add(new OtherChunk(chunk));
                        break;
                }
            } while (chunk.ChunkType != "IEND");

            // We have one more thing to do:
            // If the default image is part of the animation,
            // we should insert it into frames list.
            if (defaultImage.fcTLChunk != null)
            {
                frames.Insert(0, defaultImage);
                DefaultImageIsAnimated = true;
            }
            else //If it isn't animated it still needs the other chunks.
            {
                otherChunks.ForEach(defaultImage.AddOtherChunk);
            }

            // Now we should apply every chunk in otherChunks to every frame.
            frames.ForEach(f => otherChunks.ForEach(f.AddOtherChunk));
        }

        /// <summary>
        ///     Indicate whether the file is a simple PNG.
        /// </summary>
        public bool IsSimplePNG { get; private set; }

        /// <summary>
        ///     Indicate whether the default image is part of the animation
        /// </summary>
        public bool DefaultImageIsAnimated { get; private set; }

        /// <summary>
        ///     Gets the base image.
        ///     If IsSimplePNG = True, returns the only image;
        ///     if False, returns the default image
        /// </summary>
        public Frame DefaultImage
        {
            get { return defaultImage; }
        }

        /// <summary>
        ///     Gets the frame array.
        ///     If IsSimplePNG = True, returns empty
        /// </summary>
        public Frame[] Frames
        {
            get { return IsSimplePNG ? new Frame[]{defaultImage}:frames.ToArray(); }
        }

        /// <summary>
        /// Gets the dispose operation for the specified frame.
        /// </summary>
        /// <returns>The dispose operation for the specified frame.</returns>
        /// <param name="index">Index.</param>
        public DisposeOps GetDisposeOperationFor(int index)
        {
            return IsSimplePNG ? DisposeOps.APNGDisposeOpNone : this.frames[index].fcTLChunk.DisposeOp;
        }

        /// <summary>
        /// Gets the blend operation for the specified frame.
        /// </summary>
        /// <returns>The blend operation for the specified frame.</returns>
        /// <param name="index">Index.</param>
        public BlendOps GetBlendOperationFor(int index)
        {
            return IsSimplePNG ? BlendOps.APNGBlendOpSource : this.frames[index].fcTLChunk.BlendOp;
        }

        /// <summary>
        /// Gets the default image.
        /// </summary>
        /// <returns>The default image.</returns>
        public Bitmap GetDefaultImage()
        {
            if (IsSimplePNG)
            {
                return DefaultImage.ToBitmap();
            }
            else
            {
                return this[0];
            }
        }

        /// <summary>
        /// Gets the bitmap at the specified index.
        /// </summary>
        /// <param name="index">The frame index.</param>
        public Bitmap this[int index]
        {
            get
            {
                Bitmap bmp = null;
                if (IsSimplePNG) return new Bitmap(defaultImage.ToBitmap(), this.viewSize);
                if (index >= 0 && index < frames.Count)
                {
                    //Return bitmap of requested view size
                    bmp = new Bitmap(frames[index].ToBitmap(), this.viewSize);
                }
                return bmp;
            }
        }

        /// <summary>
        /// Gets the frame count.
        /// </summary>
        /// <value>The frame count.</value>
        public int FrameCount
        {
            get
            {
                return (int)(acTLChunk?.FrameCount??1);
            }
        }

        /// <summary>
        /// Gets the frame rate from the first frame as a global frame rate.
        /// Sets the framerate across all frames.
        /// </summary>
        /// <value>The global frame rate.</value>
        public int FrameRate
        {
            get
            {
                return GetFrameRate(0);
            }
            set
            {
                for(int i = 0; i < frames.Count; ++i)
                {
                    SetFrameRate(i, value);
                }
            }
        }

        /// <summary>
        /// Gets the frame rate for a frame.
        /// </summary>
        /// <returns>The frame rate for a frame.</returns>
        /// <param name="index">The frame index.</param>
        public int GetFrameRate(int index)
        {
            int frameRate = 0;
            if(this.frames != null && this.frames.Count > index)
            {
                frameRate = this.frames[index].FrameRate;
            }
            return frameRate;
        }

        /// <summary>
        /// Sets the frame rate for a frame.
        /// </summary>
        /// <param name="index">The frame index</param>
        /// <param name="frameRate">The desired frame rate.</param>
        public void SetFrameRate(int index, int frameRate)
        {
            if(this.frames != null && this.frames.Count > index)
            {
                this.frames[index].FrameRate = frameRate;
            }
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size of the displayed animated image.</value>
        public Size ViewSize
        {
            get
            {
                return this.viewSize;
            }
            set
            {
                this.viewSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the actual size.
        /// </summary>
        /// <value>The actual size.</value>
        public Size ActualSize
        {
            get
            {
                return new Size(this.IHDRChunk.Width, this.IHDRChunk.Height);
            }
            set
            {
                this.IHDRChunk.Width = value.Width;
                this.IHDRChunk.Height = value.Height;
            }
        }

        /// <summary>
        /// Gets and sets the play count.
        /// </summary>
        /// <value>The play count.</value>
        public int PlayCount
        {
            get
            {
                return (int)(acTLChunk?.PlayCount??0);
            }
            set
            {
                this.acTLChunk.PlayCount = (uint)value;
            }
        }

        /// <summary>
        ///     Gets the IHDR Chunk
        /// </summary>
        internal IHDRChunk IHDRChunk { get; private set; }

        /// <summary>
        ///     Gets the acTL Chunk
        /// </summary>
        internal acTLChunk acTLChunk { get; private set; }

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
    }
}
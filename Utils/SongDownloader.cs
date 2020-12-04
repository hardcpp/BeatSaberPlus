using BeatSaverSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSaberPlus.Utils
{
    /*
       Code from https://github.com/Kylemc1413/BeatSaverDownloader

       MIT License

       Copyright (c) 2019 andruzzzhka

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */

    /// <summary>
    /// Song downloader helper
    /// </summary>
    public class SongDownloader
    {
        /// <summary>
        /// Download a song
        /// </summary>
        /// <param name="p_Song">Beat map</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <param name="p_Direct">Skip download counter ?</param>
        /// <returns></returns>
        public static async Task<bool> DownloadSong(Beatmap p_Song, CancellationToken p_Token, IProgress<double> p_Progress = null, bool p_Direct = false)
        {
            try
            {
                string l_CustomSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;

                if (!Directory.Exists(l_CustomSongsPath))
                    Directory.CreateDirectory(l_CustomSongsPath);

                var l_ZIPBytes = await p_Song.DownloadZip(p_Direct, p_Token, p_Progress).ConfigureAwait(false);

                Logger.Instance?.Info("Downloaded zip!");

                return await ExtractZipAsync(p_Song, l_ZIPBytes, l_CustomSongsPath).ConfigureAwait(false);
            }
            catch (Exception p_Exception)
            {
                if (p_Exception is TaskCanceledException)
                    Logger.Instance?.Warn("Song Download Aborted.");
                else
                    Logger.Instance?.Critical("Failed to download Song!");
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Extract ZIP archive
        /// </summary>
        /// <param name="p_Song">Beat map</param>
        /// <param name="p_ZIPBytes">Raw ZIP bytes</param>
        /// <param name="p_CustomSongsPath">Extract path</param>
        /// <param name="p_Overwrite">Should overwrite ?</param>
        /// <returns></returns>
        private static async Task<bool> ExtractZipAsync(Beatmap p_Song, byte[] p_ZIPBytes, string p_CustomSongsPath, bool p_Overwrite = false)
        {
            Stream l_ZIPStream = new MemoryStream(p_ZIPBytes);

            try
            {
                Logger.Instance?.Info("Extracting...");

                /// Create ZIP archive
                ZipArchive l_ZIPArchive = new ZipArchive(l_ZIPStream, ZipArchiveMode.Read);

                /// Prepare base path
                string l_BasePath = p_Song.Key + " (" + p_Song.Metadata.SongName + " - " + p_Song.Metadata.LevelAuthorName + ")";
                l_BasePath = string.Join("", l_BasePath.Split((Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray())));

                /// Build out path
                string l_OutPath = p_CustomSongsPath + "/" + l_BasePath;

                /// Check to avoid overwrite
                if (!p_Overwrite && Directory.Exists(l_OutPath))
                {
                    int l_FolderCount = 1;

                    while (Directory.Exists(l_OutPath + $" ({l_FolderCount})"))
                        ++l_FolderCount;

                    l_OutPath += $" ({l_FolderCount})";
                }

                /// Create directory if needed
                if (!Directory.Exists(l_OutPath))
                    Directory.CreateDirectory(l_OutPath);

                Logger.Instance?.Info(l_OutPath);

                await Task.Run(() =>
                {
                    foreach (var l_Entry in l_ZIPArchive.Entries)
                    {
                        /// Name instead of FullName for better security and because song zips don't have nested directories anyway
                        var l_EntryPath = Path.Combine(l_OutPath, l_Entry.Name);

                        /// Either we're overwriting or there's no existing file
                        if (p_Overwrite || !File.Exists(l_EntryPath))
                            l_Entry.ExtractToFile(l_EntryPath, p_Overwrite);
                    }
                }).ConfigureAwait(false);

                l_ZIPArchive.Dispose();
                l_ZIPStream.Close();

                return true;
            }
            catch (Exception p_Exception)
            {
                Logger.Instance?.Critical($"Unable to extract ZIP! Exception: {p_Exception}");
                l_ZIPStream.Close();
            }

            return false;
        }
    }
}

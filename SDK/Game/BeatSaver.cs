using BeatSaverSharp;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// BeatSaver helper
    /// </summary>
    public class BeatSaver
    {
        /// <summary>
        /// Cache folder
        /// </summary>
        private static string m_CacheFolder = "UserData/BeatSaberPlus/Cache/BeatSaver/";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get beatmap from cache
        /// </summary>
        /// <param name="p_BeatSaver">Beatsaver client</param>
        /// <param name="p_Key">Beatmap key</param>
        /// <returns></returns>
        public static Beatmap GetBeatmapFromCacheByKey(BeatSaverSharp.BeatSaver p_BeatSaver, string p_Key)
        {
            var l_Path = m_CacheFolder + p_Key.ToLower() + ".json";

            try
            {
                if (!File.Exists(l_Path))
                    return null;

                var l_Content = File.ReadAllText(l_Path, Encoding.UTF8);
                var l_Result = new Beatmap(p_BeatSaver, p_Key);

                JsonConvert.PopulateObject(l_Content, l_Result);

                return l_Result;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.Game][BeatSaver.CacheBeatmap] Error :");
                Logger.Instance.Error(l_Exception);
            }

            return null;
        }
        /// <summary>
        /// Get beatmap cover from cache
        /// </summary>
        /// <param name="p_Key">Beatmap key</param>
        /// <returns></returns>
        public static byte[] GetBeatmapCoverFromCacheByKey(string p_Key)
        {
            var l_Path = m_CacheFolder + p_Key.ToLower() + ".jpg";

            try
            {
                if (!File.Exists(l_Path))
                    return null;

                var l_Result = File.ReadAllBytes(l_Path);

                return l_Result;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.Game][BeatSaver.CacheBeatmap] Error :");
                Logger.Instance.Error(l_Exception);
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cache beatmap
        /// </summary>
        /// <param name="p_Beatmap">Beatmap to cache</param>
        public static void CacheBeatmap(Beatmap p_Beatmap)
        {
            if (p_Beatmap == null || p_Beatmap.Partial)
                return;

            try
            {
                var l_JSON = JsonConvert.SerializeObject(p_Beatmap, Formatting.Indented);
                Unity.MainThreadInvoker.Enqueue(() => SharedCoroutineStarter.instance.StartCoroutine(WriteCacheTextFile(p_Beatmap.Key.ToLower() + ".json", l_JSON)));
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[SDK.Game][BeatSaver.CacheBeatmap] Error :");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Cache beatmap cover
        /// </summary>
        /// <param name="p_Beatmap">Beatmap</param>
        /// <param name="p_Cover">Cover bytes</param>
        public static void CacheBeatmapCover(Beatmap p_Beatmap, byte[] p_Cover)
        {
            if (p_Beatmap == null || p_Beatmap.Key == null || p_Cover == null || p_Cover.Length == 0)
                return;

            Unity.MainThreadInvoker.Enqueue(() => SharedCoroutineStarter.instance.StartCoroutine(WriteCacheFile(p_Beatmap.Key.ToLower() + ".jpg", p_Cover)));
        }
        /// <summary>
        /// Clear cache for a beatmap
        /// </summary>
        /// <param name="p_Key">Beatmap key</param>
        public static void ClearBeatmapCache(string p_Key)
        {
            Unity.MainThreadInvoker.Enqueue(() => {
                SharedCoroutineStarter.instance.StartCoroutine(DeleteCacheFile(p_Key.ToLower() + ".jpg"));
                SharedCoroutineStarter.instance.StartCoroutine(DeleteCacheFile(p_Key.ToLower() + ".json"));
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Write cache text file
        /// </summary>
        /// <param name="p_FileName">Cache ID</param>
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static IEnumerator WriteCacheTextFile(string p_FileName, string p_Content)
        {
            /// Wait until menu scene
            yield return new WaitUntil(() => Logic.ActiveScene == Logic.SceneType.Menu);

            Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(m_CacheFolder))
                        Directory.CreateDirectory(m_CacheFolder);

                    File.WriteAllText(m_CacheFolder + p_FileName, p_Content, Encoding.UTF8);
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance.Error("[SDK.Game][BeatSaver.WriteCacheTextFile] Error :");
                    Logger.Instance.Error(l_Exception);
                }
            });
        }
        /// <summary>
        /// Write cache file
        /// </summary>
        /// <param name="p_FileName">Cache ID</param>
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static IEnumerator WriteCacheFile(string p_FileName, byte[] p_Content)
        {
            /// Wait until menu scene
            yield return new WaitUntil(() => Logic.ActiveScene == Logic.SceneType.Menu);

            Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(m_CacheFolder))
                        Directory.CreateDirectory(m_CacheFolder);

                    File.WriteAllBytes(m_CacheFolder + p_FileName, p_Content);
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance.Error("[SDK.Game][BeatSaver.WriteCacheFile] Error :");
                    Logger.Instance.Error(l_Exception);
                }
            });
        }
        /// <summary>
        /// Delete cache file coroutine
        /// </summary>
        /// <param name="p_FileName">File to delete</param>
        /// <returns></returns>
        private static IEnumerator DeleteCacheFile(string p_FileName)
        {
            /// Wait until menu scene
            yield return new WaitUntil(() => Logic.ActiveScene == Logic.SceneType.Menu);

            Task.Run(() =>
            {
                try
                {
                    if (File.Exists(m_CacheFolder + p_FileName))
                        File.Delete(m_CacheFolder + p_FileName);
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance.Error("[SDK.Game][BeatSaver.DeleteCacheFile] Error :");
                    Logger.Instance.Error(l_Exception);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sanitize game mode
        /// </summary>
        /// <param name="p_Str">Input game mode</param>
        /// <returns></returns>
        public static string SanitizeGameMode(string p_Str)
        {
            switch (p_Str)
            {
                case "Standard": return "Standard";

                case "One Saber":
                case "OneSaber": return "OneSaber";

                case "No Arrows":
                case "NoArrows": return "NoArrows";
                case "360Degree": return "360Degree";
                case "Lawless": return "Lawless";
                case "90Degree": return "90Degree";

                case "LightShow":
                case "Lightshow": return "Lightshow";
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a fake CustomBeatmapLevel from BeatSaver BeatMap
        /// </summary>
        /// <param name="p_BeatMap">BeatMap instance</param>
        /// <returns></returns>
        public static CustomBeatmapLevel CreateFakeCustomBeatmapLevelFromBeatMap(BeatSaverSharp.Beatmap p_BeatMap)
        {
            if (p_BeatMap == null)
                return null;

            return Internal.BeatSaver_CustomBeatmapLevel.FromBeatSaver(p_BeatMap);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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

            p_Token.ThrowIfCancellationRequested();

            try
            {
                string l_CustomSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;

                if (!Directory.Exists(l_CustomSongsPath))
                    Directory.CreateDirectory(l_CustomSongsPath);

                var l_ZIPBytes = await p_Song.ZipBytes(p_Direct, new StandardRequestOptions() { Token = p_Token, Progress = p_Progress }).ConfigureAwait(false);
                if (l_ZIPBytes == null || l_ZIPBytes.Length == 0)
                    return false;

                Logger.Instance?.Info("[SDK.Game][BeatSaver] Downloaded zip!");

                return await ExtractZipAsync(p_Token, p_Song, l_ZIPBytes, l_CustomSongsPath).ConfigureAwait(false);
            }
            catch (Exception p_Exception)
            {
                if (p_Exception is TaskCanceledException)
                {
                    Logger.Instance?.Warn("[SDK.Game][BeatSaver] Song Download Aborted.");
                    throw p_Exception;
                }
                else
                    Logger.Instance?.Critical("[SDK.Game][BeatSaver] Failed to download Song!");
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
        private static async Task<bool> ExtractZipAsync(CancellationToken p_Token, Beatmap p_Song, byte[] p_ZIPBytes, string p_CustomSongsPath, bool p_Overwrite = false)
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

            p_Token.ThrowIfCancellationRequested();

            Stream l_ZIPStream = new MemoryStream(p_ZIPBytes);

            try
            {
                Logger.Instance?.Info("[SDK.Game][BeatSaver] Extracting...");

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

                Logger.Instance?.Info("[SDK.Game][BeatSaver] " + l_OutPath);

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
                l_ZIPStream.Close();

                if (p_Exception is TaskCanceledException)
                    throw p_Exception;

                Logger.Instance?.Critical("[SDK.Game][BeatSaver] Unable to extract ZIP! Exception");
                Logger.Instance?.Critical(p_Exception);
            }

            return false;
        }
    }
}

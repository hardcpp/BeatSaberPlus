using CP_SDK.Unity.OpenType;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Open type to TextMeshPro font manager
    /// </summary>
    public static class FontManager
    {
        private class FontInfo
        {
            internal string       Path;
            internal OpenTypeFont Info;

            internal FontInfo(string p_Path, OpenTypeFont p_OpenTypeFont)
            {
                Path = p_Path;
                Info = p_OpenTypeFont;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static string[] m_OSPaths = Font.GetPathsToOSFonts();
        private static Dictionary<string, List<FontInfo>> m_FontsByFamily;
        private static Dictionary<string, FontInfo> m_FontsByFullName;
        private static Dictionary<string, Font> m_FontsByPath = new Dictionary<string, Font>();
        private static Func<TMP_FontAsset, TMP_FontAsset> m_FontClone;
        private static TMP_FontAsset m_MainFont = null;
        private static TMP_FontAsset m_ChatFont = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The <see cref="Task"/> associated with an ongoing call to <see cref="AsyncLoadSystemFonts"/>.
        /// </summary>
        public static Task SystemFontLoadTask { get; private set; }
        /// <summary>
        /// Gets whether or not <see cref="FontManager"/> is initialized.
        /// </summary>
        public static bool IsInitialized => m_FontsByFamily != null;
        /// <summary>
        /// Name of the main font
        /// </summary>
        public static string MainFontName => "Segoe UI";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Setup for specific app
        /// </summary>
        /// <param name="p_FontClone">Font clone function</param>
        internal static void Setup(Func<TMP_FontAsset, TMP_FontAsset> p_FontClone)
            => m_FontClone = p_FontClone;
        /// <summary>
        /// Asynchronously loads all of the installed system fonts into <see cref="FontManager"/>.
        /// </summary>
        /// <returns>a task representing the async operation</returns>
        internal static Task Init()
        {
            if (IsInitialized)
                return Task.CompletedTask;

            if (SystemFontLoadTask != null)
                return SystemFontLoadTask;

            var l_BaseTask = Task.Factory.StartNew(LoadSystemFonts).Unwrap();

            SystemFontLoadTask = l_BaseTask.ContinueWith(t =>
                {
                    ChatPlexSDK.Logger.Debug("Font loading complete");

                    Interlocked.CompareExchange(ref m_FontsByFullName, t.Result.fulls, null);
                    Interlocked.CompareExchange(ref m_FontsByFamily, t.Result.families, null);

                    return Task.CompletedTask;
                },
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion
            ).Unwrap();

            l_BaseTask.ContinueWith(
                t => ChatPlexSDK.Logger.Error($"Font loading errored: {t.Exception}"),
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnRanToCompletion
            );
            return SystemFontLoadTask;
        }
        /// <summary>
        /// Adds a specified OpenType file to the font manager for lookup by name.
        /// </summary>
        /// <param name="p_Path">the path to add to the manager</param>
        public static Font AddFontFile(string p_Path)
        {
            ThrowIfNotInitialized();

            lock (m_FontsByFamily)
            {
                var l_Result = AddFontFileToCache(m_FontsByFamily, m_FontsByFullName, p_Path);
                if (!l_Result.Any())
                    throw new ArgumentException("File is not an OpenType font or collection", nameof(p_Path));

                return GetFontFromCacheOrLoad(l_Result.First());
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get main font
        /// </summary>
        /// <returns></returns>
        public static TMP_FontAsset GetMainFont()
        {
            if (m_MainFont != null)
                return m_MainFont;

            if (!IsInitialized)
            {
                var l_Task = Init();
                if (!l_Task.IsCompleted)
                    l_Task.Wait();
            }

            TryGetTMPFontByFamily(MainFontName, out var l_Font);
            if (m_FontClone != null)
                m_MainFont = m_FontClone(l_Font);
            else
                m_MainFont = UnityEngine.Object.Instantiate(l_Font);

            return l_Font;
        }
        /// <summary>
        /// Get chat font
        /// </summary>
        /// <returns></returns>
        public static TMP_FontAsset GetChatFont()
        {
            if (m_ChatFont != null)
                return m_ChatFont;

            var l_MainFont = GetMainFont();
            if (m_FontClone != null)
                m_ChatFont = m_FontClone(l_MainFont);
            else
                m_ChatFont = UnityEngine.Object.Instantiate(l_MainFont);

            return m_ChatFont;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to get a <see cref="TMP_FontAsset"/> with the given family name, and optionally subfamily.
        /// </summary>
        /// <param name="p_Family">the name of the font family to look for</param>
        /// <param name="p_Font">the font with that family name, if any</param>
        /// <param name="p_SubFamily">the font subfamily name</param>
        /// <param name="p_FallbackIfNoSubfamily">whether or not to fallback to the first font with the given family name if the given subfamily name was not found</param>
        /// <param name="p_SetupOsFallbacks">whether or not to set up the fallbacks specified by the OS</param>
        /// <returns><see langword="true"/> if the font was found, <see langword="false"/> otherwise</returns>
        public static bool TryGetTMPFontByFamily(string p_Family, out TMP_FontAsset p_Font, string p_SubFamily = null, bool p_FallbackIfNoSubfamily = false, bool p_SetupOsFallbacks = true)
        {
            if (!TryGetFontInfoByFamily(p_Family, out var info, p_SubFamily, p_FallbackIfNoSubfamily))
            {
                p_Font = null;
                return false;
            }

            p_Font = GetOrSetupTMPFontFor(info, p_SetupOsFallbacks);
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static async Task<(Dictionary<string, List<FontInfo>> families, Dictionary<string, FontInfo> fulls)> LoadSystemFonts()
        {
            var l_Families  = new Dictionary<string, List<FontInfo>>(m_OSPaths.Length, StringComparer.InvariantCultureIgnoreCase);
            var l_FullNames = new Dictionary<string, FontInfo>      (m_OSPaths.Length, StringComparer.InvariantCultureIgnoreCase);

            for (int l_I = 0; l_I < m_OSPaths.Length; ++l_I)
            {
                try
                {
                    AddFontFileToCache(l_Families, l_FullNames, m_OSPaths[l_I]);
                }
                catch (Exception l_Exceptions)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][FontManager.LoadSystemFonts] Error:");
                    ChatPlexSDK.Logger.Error(l_Exceptions);
                }

                await Task.Yield();
            }

            return (l_Families, l_FullNames);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static IEnumerable<FontInfo> AddFontFileToCache(Dictionary<string, List<FontInfo>> p_Cache, Dictionary<string, FontInfo> p_FullCache, string p_Path)
        {
            FontInfo AddFont(OpenTypeFont p_Font)
            {
                var l_FontInfo = new FontInfo(p_Path, p_Font);

                var l_TargetList = GetListForFamily(p_Cache, p_Font.Family);
                l_TargetList.Add(l_FontInfo);

                if (!p_FullCache.ContainsKey(p_Font.FullName))
                    p_FullCache.Add(p_Font.FullName, l_FontInfo);

                return l_FontInfo;
            }

            var l_FileStream    = new FileStream(p_Path, FileMode.Open, FileAccess.Read);
            var l_Reader        = OpenTypeReader.For(l_FileStream);

            if (l_Reader is OpenTypeCollectionReader l_CollectionReader)
            {
                var l_Collection = new OpenTypeCollection(l_CollectionReader, lazyLoad: false);
                return l_Collection.Select(AddFont).ToList();
            }
            else if (l_Reader is OpenTypeFontReader l_FontReader)
            {
                var l_Font = new OpenTypeFont(l_FontReader, lazyLoad: false);
                return Enumerable.Empty<FontInfo>().Append<FontInfo>(AddFont(l_Font));
            }
            else
            {
                ChatPlexSDK.Logger.Warning($"Font file '{p_Path}' is not an OpenType file");
                return Enumerable.Empty<FontInfo>();
            }
        }
        private static List<FontInfo> GetListForFamily(Dictionary<string, List<FontInfo>> p_Cache, string p_Family)
        {
            if (!p_Cache.TryGetValue(p_Family, out var l_list))
                p_Cache.Add(p_Family, l_list = new List<FontInfo>());

            return l_list;
        }
        private static Font GetFontFromCacheOrLoad(FontInfo p_FontInfo)
        {
            lock (m_FontsByPath)
            {
                if (!m_FontsByPath.TryGetValue(p_FontInfo.Path, out var l_Font))
                {
                    l_Font = new Font(p_FontInfo.Path);
                    l_Font.name = p_FontInfo.Info.FullName;

                    m_FontsByPath.Add(p_FontInfo.Path, l_Font);
                }

                return l_Font;
            }
        }
        private static bool TryGetFontInfoByFamily(string p_Family, out FontInfo p_FontInfo, string p_SubFamily = null, bool p_FallbackIfNoSubfamily = false)
        {
            ThrowIfNotInitialized();

            if (p_SubFamily == null)
            {
                p_FallbackIfNoSubfamily = true;
                p_SubFamily             = "Regular";
            }

            lock (m_FontsByFamily)
            {
                if (m_FontsByFamily.TryGetValue(p_Family, out var l_FamilyFonts))
                {
                    p_FontInfo = l_FamilyFonts.FirstOrDefault(p => p?.Info.Subfamily == p_SubFamily);
                    if (p_FontInfo == null)
                    {
                        if (!p_FallbackIfNoSubfamily)
                            return false;
                        else
                            p_FontInfo = l_FamilyFonts.First();
                    }

                    return true;
                }
                else
                {
                    p_FontInfo = null;
                    return false;
                }
            }
        }
        private static bool TryGetFontInfoByFullName(string p_FullName, out FontInfo p_FontInfo)
        {
            ThrowIfNotInitialized();

            lock (m_FontsByFamily)
                return m_FontsByFullName.TryGetValue(p_FullName, out p_FontInfo);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attempts to get a <see cref="TMP_FontAsset"/> by its font's full name.
        /// </summary>
        /// <param name="p_FullName">the full name of the font to look for</param>
        /// <param name="p_Font">the font identified by <paramref name="fullName"/>, if any</param>
        /// <param name="p_SetupOsFallbacks">whether or not to set up the fallbacks specified by the OS</param>
        /// <returns><see langword="true"/> if the font was found, <see langword="false"/> otherwise</returns>
        private static bool TryGetTMPFontByFullName(string p_FullName, out TMP_FontAsset p_Font, bool p_SetupOsFallbacks = true)
        {
            if (!TryGetFontInfoByFullName(p_FullName, out var info))
            {
                p_Font = null;
                return false;
            }

            p_Font = GetOrSetupTMPFontFor(info, p_SetupOsFallbacks);
            return true;
        }
        private static TMP_FontAsset GetOrSetupTMPFontFor(FontInfo p_FontInfo, bool p_SetupOsFallbacks)
        {
            var l_Font      = GetFontFromCacheOrLoad(p_FontInfo);
            var l_TempFont  = TMP_FontAsset.CreateFontAsset(l_Font);

            l_TempFont.name     = p_FontInfo.Info.FullName ?? l_Font.name;
            l_TempFont.hashCode = TMP_TextUtilities.GetSimpleHashCode(l_Font.name);

            if (p_SetupOsFallbacks)
            {
                var l_Fallbacks = GetOSFontFallbackList(p_FontInfo.Info.FullName);
                foreach (var l_CurrentFallback in l_Fallbacks)
                {
                    if (!TryGetTMPFontByFullName(l_CurrentFallback, out var l_FallbackFont, false))
                        continue;

                    if (l_TempFont.fallbackFontAssetTable == null)
                        l_TempFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

                    l_TempFont.fallbackFontAssetTable.Add(l_FallbackFont);
                }
            }

            return l_TempFont;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the font fallback list provided by the OS for a given font name, if there is any.
        /// </summary>
        /// <param name="fullname">the full name of the font to look up the fallbacks for</param>
        /// <returns>a list of fallbacks defined by the OS</returns>
        private static IEnumerable<string> GetOSFontFallbackList(string fullname)
        {
            var l_SyslinkKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontLink\SystemLink");
            if (l_SyslinkKey == null)
                return Enumerable.Empty<string>();

            var l_Value = l_SyslinkKey.GetValue(fullname);
            if (l_Value is string[] l_Names)
            {
                /// the format in this is '<filename>,<font full name>[,<some other stuff>]'
                return l_Names
                    .Select(s => s.Split(','))
                    .Select(a => a.Length > 1 ? a[1] : null)
                    .Where(s => s != null);
            }

            return Enumerable.Empty<string>();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("FontManager not initialized");
        }
    }
}
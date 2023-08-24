using CP_SDK.Unity.OpenType;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

        private static object                               m_LockObject                = new object();
        private static string[]                             m_OSPaths                   = Font.GetPathsToOSFonts();
        private static Dictionary<string, List<FontInfo>>   m_FontsInfoByFamily         = new Dictionary<string, List<FontInfo>>();
        private static Dictionary<string, FontInfo>         m_FontsInfoByFullName       = new Dictionary<string, FontInfo>();
        private static Dictionary<string, Font>             m_FontsByFullName           = new Dictionary<string, Font>();
        private static Dictionary<string, Font>             m_FontsByPath               = new Dictionary<string, Font>();
        private static Dictionary<FontInfo, TMP_FontAsset>  m_TMPFontAssetsByFontInfoA  = new Dictionary<FontInfo, TMP_FontAsset>();
        private static Dictionary<FontInfo, TMP_FontAsset>  m_TMPFontAssetsByFontInfoB  = new Dictionary<FontInfo, TMP_FontAsset>();

        private static Func<TMP_FontAsset, TMP_FontAsset> m_TMPFontAssetSetup;
        private static TMP_FontAsset m_MainFont = null;
        private static TMP_FontAsset m_ChatFont = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Task      SystemFontLoadTask  { get; private set; }
        public static bool      IsInitialized       { get; private set; } = false;
        public static string    MainFontName        => "Segoe UI";
        public static string    ChatFontName        => "Segoe UI";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Setup for specific app
        /// </summary>
        /// <param name="p_TMPFontAssetSetup">Font setup function</param>
        internal static void Setup(Func<TMP_FontAsset, TMP_FontAsset> p_TMPFontAssetSetup)
            => m_TMPFontAssetSetup = p_TMPFontAssetSetup;
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
                    IsInitialized = true;
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

            TryGetTMPFontAssetByFamily(MainFontName, out var l_MainFontOG);
            if (l_MainFontOG)
            {
                m_MainFont = TMP_FontAsset.CreateFontAsset(l_MainFontOG.sourceFontFile);
                m_MainFont.name                     = l_MainFontOG.name + " Clone" + Misc.Time.UnixTimeNowMS().ToString();
                m_MainFont.hashCode                 = TMP_TextUtilities.GetSimpleHashCode(m_MainFont.name);
                m_MainFont.fallbackFontAssetTable   = l_MainFontOG.fallbackFontAssetTable;

                if (m_TMPFontAssetSetup != null)
                    m_MainFont = m_TMPFontAssetSetup(m_MainFont);

                m_MainFont.normalStyle          =  0.5f;
                m_MainFont.normalSpacingOffset  = -1.0f;
                m_MainFont.boldStyle            =  2.0f;
                m_MainFont.boldSpacing          =  2.0f;
                m_MainFont.italicStyle          = 15;
            }
            return m_MainFont;
        }
        /// <summary>
        /// Get chat font
        /// </summary>
        /// <returns></returns>
        public static TMP_FontAsset GetChatFont()
        {
            if (m_ChatFont != null)
                return m_ChatFont;

            if (!IsInitialized)
            {
                var l_Task = Init();
                if (!l_Task.IsCompleted)
                    l_Task.Wait();
            }

            TryGetTMPFontAssetByFamily(ChatFontName, out m_ChatFont);

            /// Clean reserved characters
            m_ChatFont.characterTable.RemoveAll(x => x.glyphIndex > 0xE000 && x.glyphIndex <= 0xF8FF);
            m_ChatFont.characterTable.RemoveAll(x => x.glyphIndex > 0xF0000);

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
        public static bool TryGetTMPFontAssetByFamily(string p_Family, out TMP_FontAsset p_Font, string p_SubFamily = null, bool p_FallbackIfNoSubfamily = false, bool p_SetupOsFallbacks = true)
        {
            if (!TryGetFontInfoByFamily(p_Family, out var info, p_SubFamily, p_FallbackIfNoSubfamily))
            {
                p_Font = null;
                return false;
            }

            p_Font = GetTMPFontAssetFromCacheOrLoad(info, p_SetupOsFallbacks);
            return true;
        }
        /// <summary>
        /// Attempts to get a <see cref="TMP_FontAsset"/> by its font's full name.
        /// </summary>
        /// <param name="p_FullName">the full name of the font to look for</param>
        /// <param name="p_Font">the font identified by <paramref name="fullName"/>, if any</param>
        /// <param name="p_SetupOsFallbacks">whether or not to set up the fallbacks specified by the OS</param>
        /// <returns><see langword="true"/> if the font was found, <see langword="false"/> otherwise</returns>
        public static bool TryGetTMPFontAssetByFullName(string p_FullName, out TMP_FontAsset p_Font, bool p_SetupOsFallbacks = true)
        {
            if (!TryGetFontInfoByFullName(p_FullName, out var p_FontInfo))
            {
                p_Font = null;
                return false;
            }

            p_Font = GetTMPFontAssetFromCacheOrLoad(p_FontInfo, p_SetupOsFallbacks);
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds a specified OpenType file to the font manager for lookup by name.
        /// </summary>
        /// <param name="p_Path">the path to add to the manager</param>
        public static bool AddFontFile(string p_Path, out string p_FamilyName)
        {
            p_FamilyName = string.Empty;

            ThrowIfNotInitialized();

            var l_Result = AddFontFileToCache(p_Path);
            if (l_Result == null)
                return false;

            p_FamilyName = l_Result.Info.Family;
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load system fonts
        /// </summary>
        /// <returns></returns>
        private static async Task LoadSystemFonts()
        {
            for (int l_I = 0; l_I < m_OSPaths.Length; ++l_I)
            {
                try
                {
                    AddFontFileToCache(m_OSPaths[l_I]);
                }
                catch (Exception l_Exceptions)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][FontManager.LoadSystemFonts] Error:");
                    ChatPlexSDK.Logger.Error(l_Exceptions);
                }

                await Task.Yield();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add font file to cache
        /// </summary>
        /// <param name="p_Path">Path</param>
        /// <returns></returns>
        private static FontInfo AddFontFileToCache(string p_Path)
        {
            var l_FileStream    = new FileStream(p_Path, FileMode.Open, FileAccess.Read);
            var l_Reader        = OpenTypeReader.For(l_FileStream);

            if (l_Reader is OpenTypeCollectionReader l_CollectionReader)
            {
                var l_Collection    = new OpenTypeCollection(l_CollectionReader, p_LazyLoad: false);
                var l_Fonts         = l_Collection.GetFonts();
                var l_Result        = null as FontInfo;

                for (var l_I = 0; l_I < l_Fonts.Length; ++l_I)
                {
                    var l_Added = AddOpenTypeFontToCache(l_Fonts[l_I], p_Path);
                    if (l_I == 0)
                        l_Result = l_Added;
                }

                return l_Result;
            }
            else if (l_Reader is OpenTypeFontReader l_FontReader)
            {
                var l_Font = new OpenTypeFont(l_FontReader, p_LazyLoad: false);
                return AddOpenTypeFontToCache(l_Font, p_Path);
            }

            ChatPlexSDK.Logger.Warning($"[CP_SDK.Unity][FontManager.AddFontFileToCache] Font file '{p_Path}' is not an OpenType file");
            return null;
        }
        /// <summary>
        /// Add an open type font to the cache
        /// </summary>
        /// <param name="p_OpenTypeFont">OpenType font</param>
        /// <param name="p_FontPath">Font path</param>
        /// <returns></returns>
        private static FontInfo AddOpenTypeFontToCache(OpenTypeFont p_OpenTypeFont, string p_FontPath)
        {
            var l_FontInfo = new FontInfo(p_FontPath, p_OpenTypeFont);
            lock (m_LockObject)
            {
                if (!m_FontsInfoByFamily.TryGetValue(p_OpenTypeFont.Family, out var l_FamilyFontsInfoList))
                    m_FontsInfoByFamily.Add(p_OpenTypeFont.Family, new List<FontInfo>() { l_FontInfo });
                else
                    l_FamilyFontsInfoList.Add(l_FontInfo);

                if (!m_FontsInfoByFullName.ContainsKey(p_OpenTypeFont.FullName))
                    m_FontsInfoByFullName.Add(p_OpenTypeFont.FullName, l_FontInfo);
            }

            return l_FontInfo;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get font info by family name
        /// </summary>
        /// <param name="p_Family"></param>
        /// <param name="p_FontInfo">Result font info</param>
        /// <param name="p_SubFamily">Sub family name</param>
        /// <param name="p_FallbackIfNoSubfamily">Use fallback if no subfamily</param>
        /// <returns></returns>
        private static bool TryGetFontInfoByFamily(string p_Family, out FontInfo p_FontInfo, string p_SubFamily = null, bool p_FallbackIfNoSubfamily = false)
        {
            ThrowIfNotInitialized();

            p_FontInfo = null;

            if (p_SubFamily == null)
            {
                p_FallbackIfNoSubfamily = true;
                p_SubFamily             = "Regular";
            }

            lock (m_FontsInfoByFamily)
            {
                if (!m_FontsInfoByFamily.TryGetValue(p_Family, out var l_FamilyFonts))
                    return false;

                for (var l_I = 0; l_I < l_FamilyFonts.Count; ++l_I)
                {
                    if (l_FamilyFonts[l_I].Info.Subfamily != p_SubFamily)
                        continue;

                    p_FontInfo = l_FamilyFonts[l_I];
                    return true;
                }

                if (!p_FallbackIfNoSubfamily)
                    return false;

                p_FontInfo = l_FamilyFonts[0];
                return true;
            }
        }
        /// <summary>
        /// Get font info by full name
        /// </summary>
        /// <param name="p_FullName">Font full name</param>
        /// <param name="p_FontInfo">Result font info</param>
        /// <returns></returns>
        private static bool TryGetFontInfoByFullName(string p_FullName, out FontInfo p_FontInfo)
        {
            ThrowIfNotInitialized();

            lock (m_FontsInfoByFullName)
                return m_FontsInfoByFullName.TryGetValue(p_FullName, out p_FontInfo);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get front from the cache or load it
        /// </summary>
        /// <param name="p_FontInfo">Font info</param>
        /// <returns></returns>
        private static Font GetFontFromCacheOrLoad(FontInfo p_FontInfo)
        {
            var l_Font = null as Font;
            lock (m_LockObject)
            {
                if (m_FontsByPath.TryGetValue(p_FontInfo.Path, out l_Font))
                    return l_Font;

                if (m_FontsByFullName.TryGetValue(p_FontInfo.Info.FullName, out l_Font))
                    return l_Font;
            }

            l_Font      = new Font(p_FontInfo.Path);
            l_Font.name = p_FontInfo.Info.FullName;

            lock (m_LockObject)
            {
                if (!m_FontsByPath.ContainsKey(p_FontInfo.Path))
                    m_FontsByPath.Add(p_FontInfo.Path, l_Font);

                if (!m_FontsByFullName.ContainsKey(p_FontInfo.Path))
                    m_FontsByFullName.Add(p_FontInfo.Info.FullName, l_Font);
            }

            return l_Font;
        }
        /// <summary>
        /// Get a TMP_FontAsset from cache or load it
        /// </summary>
        /// <param name="p_FontInfo">Font info</param>
        /// <param name="p_SetupOsFallbacks">Should setup OS fallbacks</param>
        /// <returns></returns>
        private static TMP_FontAsset GetTMPFontAssetFromCacheOrLoad(FontInfo p_FontInfo, bool p_SetupOsFallbacks)
        {
            var l_Target = p_SetupOsFallbacks ? m_TMPFontAssetsByFontInfoA : m_TMPFontAssetsByFontInfoB;
            lock (m_LockObject)
            {
                if (l_Target.TryGetValue(p_FontInfo, out var l_TMPFontAsset))
                    return l_TMPFontAsset;
            }

            var l_Font              = GetFontFromCacheOrLoad(p_FontInfo);
            var l_NewTMPFontAsset   = TMP_FontAsset.CreateFontAsset(l_Font);
            l_NewTMPFontAsset.name      = "[CP_SDK]" + (p_FontInfo.Info.FullName ?? l_Font.name);
            l_NewTMPFontAsset.hashCode  = TMP_TextUtilities.GetSimpleHashCode(l_Font.name);

            if (p_SetupOsFallbacks)
            {
                var l_Fallbacks = GetOSFontFallbackFullNameList(p_FontInfo.Info.FullName);
                for (var l_I = 0; l_I < l_Fallbacks.Count; ++l_I)
                {
                    if (!TryGetTMPFontAssetByFullName(l_Fallbacks[l_I], out var l_FallbackFont, false))
                        continue;

                    if (l_NewTMPFontAsset.fallbackFontAssetTable == null)
                        l_NewTMPFontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();

                    if (l_NewTMPFontAsset.fallbackFontAssetTable.Contains(l_FallbackFont))
                        continue;

                    var l_FaceInfo = l_FallbackFont.faceInfo;
                    l_FaceInfo.scale = 0.85f;
                    l_FallbackFont.faceInfo = l_FaceInfo;

                    if (m_TMPFontAssetSetup != null)
                        l_FallbackFont = m_TMPFontAssetSetup(l_FallbackFont);

                    l_NewTMPFontAsset.fallbackFontAssetTable.Add(l_FallbackFont);
                }
            }

            if (m_TMPFontAssetSetup != null)
                l_NewTMPFontAsset = m_TMPFontAssetSetup(l_NewTMPFontAsset);

            lock (m_LockObject)
            {
                if (!l_Target.ContainsKey(p_FontInfo))
                    l_Target.Add(p_FontInfo, l_NewTMPFontAsset);
            }

            return l_NewTMPFontAsset;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the font fallback list provided by the OS for a given font name, if there is any.
        /// </summary>
        /// <param name="p_FullName">the full name of the font to look up the fallbacks for</param>
        private static List<string> GetOSFontFallbackFullNameList(string p_FullName)
        {
            var l_Result        = new List<string>();
            var l_SyslinkKey    = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontLink\SystemLink");

            if (l_SyslinkKey == null)
                return l_Result;

            var l_Value = l_SyslinkKey.GetValue(p_FullName);
            if (!(l_Value is string[] l_Names))
                return l_Result;

            for (var l_I = 0; l_I < l_Names.Length; ++l_I)
            {
                var l_Parts = l_Names[l_I].Split(',');
                if (l_Parts.Length < 1 || l_Result.Contains(l_Parts[1]))
                    continue;

                l_Result.Add(l_Parts[1]);
            }

            return l_Result;
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
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using BeatSaberPlus.Utils;

namespace BeatSaberPlus.Plugins.Online.Patches
{
    /// <summary>
    /// BeatSaver URL patcher
    /// </summary>
    internal class PBeatSaverSharp_BeatSaver
    {
        /// <summary>
        /// Enable or disable the patch
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetUseBeatSaberPlusCustomMapsServer(bool p_Enabled)
        {
            if (!p_Enabled || Config.Online.UseBSPCustomMapsServerOnMoreSongs)
                GetBeatSaverDownloaderInstance()?.UseBeatSaberPlusCustomMapsServer(p_Enabled);

            if (Config.ChatRequest.Enabled && ChatRequest.ChatRequest.Instance != null)
                ChatRequest.ChatRequest.Instance.OnBeatSaverProviderChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get BeatSaverDownloader BeatSaver client instance
        /// </summary>
        /// <returns></returns>
        private static BeatSaverSharp.BeatSaver GetBeatSaverDownloaderInstance()
        {
            foreach (var l_Assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (l_Assembly.FullName.StartsWith("BeatSaverDownloader,"))
                {
                    var l_PluginType = l_Assembly.GetType("BeatSaverDownloader.Plugin");
                    if (l_PluginType != null)
                        return l_PluginType.GetField("BeatSaver", BindingFlags.Static | BindingFlags.Public).GetValue(null) as BeatSaverSharp.BeatSaver;

                    break;
                }
            }

            return null;
        }
    }
}

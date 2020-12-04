using HarmonyLib;
using HMUI;
using Polyglot;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
    /// <summary>
    /// PlayerStatisticsViewController
    /// </summary>
    [HarmonyPatch(typeof(PlayerStatisticsViewController))]
    [HarmonyPatch("DidActivate")]
    public class PPlayerStatisticsViewController
    {
        /// <summary>
        /// On View controller did activate
        /// </summary>
        /// <param name="__instance">PlayerStatisticsViewController instance</param>
        /// <returns></returns>
        internal static void Postfix(ref PlayerStatisticsViewController __instance)
        {
            var l_HeaderBG = __instance.transform.Find("HeaderPanel")?.Find("BG");
            if (l_HeaderBG != null && l_HeaderBG && l_HeaderBG.GetComponent<ImageView>() && ColorUtility.TryParseHtmlString("#00000080", out var l_Color))
                l_HeaderBG.GetComponent<ImageView>().color = l_Color;
        }
    }
}

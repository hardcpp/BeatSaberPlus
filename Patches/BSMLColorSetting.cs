using HarmonyLib;
using System;

namespace BeatSaberPlus.Patches
{
    [HarmonyPatch(typeof(BeatSaberMarkupLanguage.Components.Settings.ColorSetting), nameof(BeatSaberMarkupLanguage.Components.Settings.ColorSetting.ApplyValue),
                  new Type[] { })]
    public class BSMLColorSetting_ApplyValue
    {
        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">ColorSetting instance</param>
        internal static void Postfix(ref BeatSaberMarkupLanguage.Components.Settings.ColorSetting __instance)
        {
            if (__instance.updateOnChange)
                __instance.onChange?.Invoke(__instance.CurrentColor);
        }
    }

    [HarmonyPatch(typeof(BeatSaberMarkupLanguage.Components.Settings.ColorSetting), nameof(BeatSaberMarkupLanguage.Components.Settings.ColorSetting.ReceiveValue),
                  new Type[] { })]
    public class BSMLColorSetting_ReceiveValue
    {
        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">ColorSetting instance</param>
        internal static void Postfix(ref BeatSaberMarkupLanguage.Components.Settings.ColorSetting __instance)
        {
            if (__instance.updateOnChange)
                __instance.onChange?.Invoke(__instance.CurrentColor);
        }
    }
}

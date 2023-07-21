using HarmonyLib;
using System;
using VRUIControls;

namespace BeatSaberPlus.SDK.UI.Patches
{
    [HarmonyPatch(typeof(VRPointer))]
#if BEATSABER_1_29_4_OR_NEWER
    [HarmonyPatch(nameof(VRPointer.EnabledLastSelectedPointer), new Type[] { })]
#else
    [HarmonyPatch(nameof(VRPointer.OnEnable), new Type[] { })]
#endif
    internal class PVRPointer
    {
        /// <summary>
        /// On enable event
        /// </summary>
        internal static event Action<VRPointer> OnActivated;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">VRPointer instance</param>
        internal static void Postfix(VRPointer __instance)
        {
            try { OnActivated?.Invoke(__instance); }
            catch { }
        }
    }
}

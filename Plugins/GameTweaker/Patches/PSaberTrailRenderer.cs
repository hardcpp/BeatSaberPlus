using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
    /// <summary>
    /// Saber trail remover
    /// </summary>
    [HarmonyPatch(typeof(SaberTrailRenderer))]
    [HarmonyPatch(nameof(SaberTrailRenderer.UpdateIndices))]
    public class PSaberTrailRenderer : SaberTrailRenderer
    {
        private static List<MeshRenderer> m_MeshRenderers = new List<MeshRenderer>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">SaberTrailRenderer instance</param>
        internal static void Prefix(ref SaberTrailRenderer __instance, ref MeshRenderer ____meshRenderer)
        {
            if (____meshRenderer && !m_MeshRenderers.Contains(____meshRenderer))
            {
                m_MeshRenderers.Add(____meshRenderer);
                if (Config.GameTweaker.Enabled)
                {
                    ____meshRenderer.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                    /// Apply
                    SetEnabled(Config.GameTweaker.RemoveSaberSmoothingTrail, Config.GameTweaker.SaberSmoothingTrailIntensity);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect is enabled
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetEnabled(bool p_Enabled, float p_Intensity)
        {
            List<MeshRenderer> l_ToRemove = new List<MeshRenderer>();
            foreach (var l_Current in m_MeshRenderers)
            {
                if (!l_Current)
                {
                    l_ToRemove.Add(l_Current);
                    continue;
                }

                var l_Color = l_Current.material.color;
                l_Color.a = p_Intensity;

                l_Current.enabled           = !p_Enabled;
                l_Current.material.color    = l_Color;
            }

            foreach (var l_Current in l_ToRemove)
                m_MeshRenderers.Remove(l_Current);
        }
    }
}

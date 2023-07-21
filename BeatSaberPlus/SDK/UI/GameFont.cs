using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Helpers for game font
    /// </summary>
    public static class GameFont
    {
        private static TMP_FontAsset    m_BaseGameFont                  = null;
        private static Material         m_BaseGameFontSharedMaterial    = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get main game font
        /// </summary>
        /// <returns></returns>
        public static TMP_FontAsset GetGameFont()
        {
            if (m_BaseGameFont || CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                return m_BaseGameFont;

            m_BaseGameFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Where(t => t.name == "Teko-Medium SDF").FirstOrDefault();
            return m_BaseGameFont;
        }
        /// <summary>
        /// Get main game font curved material
        /// </summary>
        /// <returns></returns>
        public static Material GetGameFontSharedMaterial()
        {
            if (m_BaseGameFontSharedMaterial || CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                return m_BaseGameFontSharedMaterial;

            m_BaseGameFontSharedMaterial = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().Where(t => t.name == "Teko-Medium SDF Curved Softer").Last());
            return m_BaseGameFontSharedMaterial;
        }
    }
}

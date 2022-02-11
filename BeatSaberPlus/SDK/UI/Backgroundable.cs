using UnityEngine;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Backgroundable helper
    /// </summary>
    public class Backgroundable
    {
        /// <summary>
        /// Set Backgroundable opacity
        /// </summary>
        /// <param name="p_Backgroundable">Backgroundable game object</param>
        /// <param name="p_Opacity">New opacity</param>
        /// <returns></returns>
        public static bool SetOpacity(GameObject p_Backgroundable, float p_Opacity)
        {
            if (p_Backgroundable == null || !p_Backgroundable)
                return false;

            var l_Image = p_Backgroundable?.GetComponent<HMUI.ImageView>() ?? null;
            if (l_Image)
            {
                /// Update background color
                var l_Color = l_Image.color;
                l_Color.a = p_Opacity;

                l_Image.color = l_Color;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Set Backgroundable opacity
        /// </summary>
        /// <param name="p_Backgroundable">Backgroundable game object</param>
        /// <param name="p_Opacity">New opacity</param>
        /// <returns></returns>
        public static bool SetOpacity(BeatSaberMarkupLanguage.Components.Backgroundable p_Backgroundable, float p_Opacity)
        {
            return SetOpacity(p_Backgroundable != null ? p_Backgroundable.gameObject : null, p_Opacity);
        }
    }
}

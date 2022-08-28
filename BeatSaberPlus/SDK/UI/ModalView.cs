using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Modal helper class
    /// </summary>
    public class ModalView
    {
        /// <summary>
        /// Setup loading control component
        /// </summary>
        /// <param name="p_Modal">Modal game object</param>
        /// <returns></returns>
        public static LoadingControl SetupLoadingControl(HMUI.ModalView p_Modal)
        {
            if (!p_Modal)
                return null;

            var l_Control = Resources.FindObjectsOfTypeAll<LoadingControl>().FirstOrDefault();
            if (l_Control == null)
                return null;

            var l_LoadingControl = GameObject.Instantiate(l_Control, p_Modal.transform);
            l_LoadingControl.transform.SetAsLastSibling();

            var l_Touchable = l_LoadingControl.GetComponent<HMUI.Touchable>();
            if (l_Touchable)
                GameObject.Destroy(l_Touchable);

            return l_LoadingControl;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set modal opacity
        /// </summary>
        /// <param name="p_Modal">Modal game object</param>
        /// <param name="p_Opacity">New opacity</param>
        /// <returns></returns>
        public static bool SetOpacity(GameObject p_Modal, float p_Opacity)
        {
            if (!p_Modal)
                return false;

            var l_BG = p_Modal.gameObject.transform.Find("BG");
            var l_Image = l_BG?.GetComponent<HMUI.ImageView>() ?? null;

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
        /// Set modal opacity
        /// </summary>
        /// <param name="p_Modal">Modal game object</param>
        /// <param name="p_Opacity">New opacity</param>
        /// <returns></returns>
        public static bool SetOpacity(HMUI.ModalView p_Modal, float p_Opacity)
        {
            return SetOpacity(p_Modal != null ? p_Modal.gameObject : null, p_Opacity);
        }
    }
}

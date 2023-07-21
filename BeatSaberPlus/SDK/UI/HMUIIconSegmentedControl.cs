using IPA.Utilities;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Vertical icon segmented control
    /// </summary>
    public static class HMUIIconSegmentedControl
    {
        /// <summary>
        /// Create icon segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <returns>GameObject</returns>
        public static HMUI.IconSegmentedControl Create(RectTransform p_Parent, bool p_HideCellBackground)
        {
            HMUI.IconSegmentedControl l_Prefab  = Resources.FindObjectsOfTypeAll<HMUI.IconSegmentedControl>().First(x => x.name == "BeatmapCharacteristicSegmentedControl" && x._container != null);
            HMUI.IconSegmentedControl l_Control = MonoBehaviour.Instantiate(l_Prefab, p_Parent, false);

            l_Control.name = "BSPIconSegmentedControl";
            l_Control.SetField("_container", l_Prefab._container);
            l_Control._hideCellBackground =  p_HideCellBackground;

            RectTransform l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = Vector2.one * 0.5f;
            l_RectTransform.anchorMax           = Vector2.one * 0.5f;
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = Vector2.one * 0.5f;

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            MonoBehaviour.Destroy(l_Control.GetComponent<BeatmapCharacteristicSegmentedControlController>());

            return l_Control;
        }
        /// <summary>
        /// Set data and remove hover hints
        /// </summary>
        /// <param name="p_Instance">Control instance</param>
        /// <param name="p_Data">Data to set</param>
        public static void SetDataNoHoverHint(this HMUI.IconSegmentedControl p_Instance, HMUI.IconSegmentedControl.DataItem[] p_Data)
        {
            p_Instance.SetData(p_Data);
            try
            {
                var l_HoverHints        = p_Instance.GetComponentsInChildren<HMUI.HoverHint>(true);
                var l_LocalHoverHints   = p_Instance.GetComponentsInChildren<LocalizedHoverHint>(true);

                foreach (var l_Current in l_HoverHints) GameObject.Destroy(l_Current);
                foreach (var l_Current in l_LocalHoverHints) GameObject.Destroy(l_Current);
            }
            catch (System.Exception)
            {

            }
        }
    }
}

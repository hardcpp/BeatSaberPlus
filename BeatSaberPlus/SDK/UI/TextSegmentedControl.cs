using IPA.Utilities;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Text segmented control
    /// </summary>
    public class TextSegmentedControl
    {
        /// <summary>
        /// Create text segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <returns>GameObject</returns>
        public static HMUI.TextSegmentedControl Create(RectTransform p_Parent, bool p_HideCellBackground, string[] p_Texts = null)
        {
            HMUI.TextSegmentedControl l_Prefab = Resources.FindObjectsOfTypeAll<HMUI.TextSegmentedControl>().First(x => x.name == "BeatmapDifficultySegmentedControl" && x.GetField<DiContainer, HMUI.TextSegmentedControl>("_container") != null);
            HMUI.TextSegmentedControl l_Control = MonoBehaviour.Instantiate((HMUI.TextSegmentedControl)l_Prefab, p_Parent, false);

            l_Control.name = "BSMLTextSegmentedControl";
            l_Control.SetField("_container", l_Prefab.GetField<DiContainer, HMUI.TextSegmentedControl>("_container"));
            l_Control.SetField("_hideCellBackground", p_HideCellBackground);

            RectTransform l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = Vector2.one * 0.5f;
            l_RectTransform.anchorMax           = Vector2.one * 0.5f;
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = Vector2.one * 0.5f;

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            MonoBehaviour.Destroy(l_Control.GetComponent<BeatmapDifficultySegmentedControlController>());

            l_Control.SetTexts(p_Texts != null ? p_Texts : new string[] { "Tab" });

            return l_Control;
        }
    }
}

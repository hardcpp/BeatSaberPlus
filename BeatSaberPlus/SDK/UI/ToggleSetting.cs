using BeatSaberMarkupLanguage.Parser;
using IPA.Utilities;
using System;
using System.Linq;
using TMPro;

using BSMLToggleSetting    = BeatSaberMarkupLanguage.Components.Settings.ToggleSetting;
using BSMLToggleSettingTag = BeatSaberMarkupLanguage.Tags.ToggleSettingTag;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Toggle setting
    /// </summary>
    public class ToggleSetting
    {
        /// <summary>
        /// ToggleSetting creator
        /// </summary>
        private static BSMLToggleSettingTag m_ToggleSettingCreator = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a toggle setting
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        /// <param name="p_Text">Toggle caption</param>
        /// <param name="p_Action">Toggle callback</param>
        /// <param name="p_HoverHint">Hover hint text</param>
        public static BSMLToggleSetting Create(UnityEngine.Transform p_Parent, string p_Text, bool p_Enabled, Action<bool> p_Action, string p_HoverHint = null)
        {
            if (m_ToggleSettingCreator == null)
                m_ToggleSettingCreator = new BSMLToggleSettingTag();

            var l_ToggleObject = m_ToggleSettingCreator.CreateObject(p_Parent);
            l_ToggleObject.gameObject.SetActive(false);

            var l_Toggle = l_ToggleObject.GetComponent<BSMLToggleSetting>();
            l_Toggle.Text   = p_Text;
            l_Toggle.Value  = p_Enabled;
            l_Toggle.toggle.onValueChanged.AddListener((x) => { p_Action(x); });

            if (!string.IsNullOrEmpty(p_HoverHint))
            {
                HMUI.HoverHint l_HoverHint = l_ToggleObject.GetComponent<HMUI.HoverHint>() ?? l_ToggleObject.AddComponent<HMUI.HoverHint>();
                l_HoverHint.text = p_HoverHint;
                l_HoverHint.SetField("_hoverHintController", UnityEngine.Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
            }

            l_ToggleObject.gameObject.SetActive(true);
            return l_Toggle;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Setup a toggle setting
        /// </summary>
        /// <param name="p_Setting">Setting to setûp</param>
        /// <param name="p_Action">Action on change</param>
        /// <param name="p_Value">New value</param>
        /// <param name="p_RemoveLabel">Should remove label</param>
        public static void Setup(BSMLToggleSetting p_Setting, BSMLAction p_Action, bool p_Value, bool p_RemoveLabel)
        {
            p_Setting.gameObject.SetActive(false);

            p_Setting.Value = p_Value;

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            if (p_RemoveLabel)
            {
                UnityEngine.GameObject.Destroy(p_Setting.gameObject.GetComponentsInChildren<TextMeshProUGUI>().ElementAt(0).transform.gameObject);

                UnityEngine.RectTransform l_RectTransform = p_Setting.gameObject.transform.GetChild(1) as UnityEngine.RectTransform;
                l_RectTransform.anchorMin = UnityEngine.Vector2.zero;
                l_RectTransform.anchorMax = UnityEngine.Vector2.one;
                l_RectTransform.sizeDelta = UnityEngine.Vector2.one;

                p_Setting.gameObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth = -1f;
            }

            p_Setting.gameObject.SetActive(true);
        }
    }
}

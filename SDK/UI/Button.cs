using IPA.Utilities;
using System;
using System.Linq;
using TMPro;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Button helper class
    /// </summary>
    public class Button
    {
        /// <summary>
        /// Button creator
        /// </summary>
        private static BeatSaberMarkupLanguage.Tags.ButtonTag m_ButtonCreator = null;
        /// <summary>
        /// Button creator
        /// </summary>
        private static Internal.BSMLPrimaryButtonTag m_PrimaryButtonCreator = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a button
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        /// <param name="p_Text">Button caption</param>
        /// <param name="p_Action">Button callback</param>
        /// <param name="p_HoverHint">Hover hint text</param>
        /// <param name="p_PreferedWidth">Prefered width</param>
        /// <param name="p_PreferedHeight">Prefered height</param>
        public static UnityEngine.UI.Button Create(UnityEngine.Transform p_Parent, string p_Text, Action p_Action, string p_HoverHint = null, float? p_PreferedWidth = null, float? p_PreferedHeight = null)
        {
            if (m_ButtonCreator == null)
                m_ButtonCreator = new BeatSaberMarkupLanguage.Tags.ButtonTag();

            var l_ButtonObject = m_ButtonCreator.CreateObject(p_Parent);
            l_ButtonObject.SetActive(false);

            l_ButtonObject.GetComponentInChildren<TextMeshProUGUI>().text = p_Text;

            var l_Button = l_ButtonObject.GetComponent<UnityEngine.UI.Button>();
            l_Button.onClick.RemoveAllListeners();
            l_Button.onClick.AddListener(() => p_Action());

            if (p_PreferedWidth.HasValue)
                l_ButtonObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth    = p_PreferedWidth.Value;
            if (p_PreferedHeight.HasValue)
                l_ButtonObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight   = p_PreferedHeight.Value;

            if (!string.IsNullOrEmpty(p_HoverHint))
            {
                HMUI.HoverHint l_HoverHint = l_ButtonObject.GetComponent<HMUI.HoverHint>() ?? l_ButtonObject.AddComponent<HMUI.HoverHint>();
                l_HoverHint.text = p_HoverHint;
                l_HoverHint.SetField("_hoverHintController", UnityEngine.Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
            }

            l_ButtonObject.SetActive(true);
            return l_ButtonObject.GetComponent<UnityEngine.UI.Button>();
        }
        /// <summary>
        /// Create a primary button
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        /// <param name="p_Text">Button caption</param>
        /// <param name="p_Action">Button callback</param>
        /// <param name="p_HoverHint">Hover hint text</param>
        /// <param name="p_PreferedWidth">Prefered width</param>
        /// <param name="p_PreferedHeight">Prefered height</param>
        public static UnityEngine.UI.Button CreatePrimary(UnityEngine.Transform p_Parent, string p_Text, Action p_Action, string p_HoverHint = null, float? p_PreferedWidth = null, float? p_PreferedHeight = null)
        {
            if (m_PrimaryButtonCreator == null)
                m_PrimaryButtonCreator = new Internal.BSMLPrimaryButtonTag();

            var l_ButtonObject = m_PrimaryButtonCreator.CreateObject(p_Parent);
            l_ButtonObject.SetActive(false);

            l_ButtonObject.GetComponentInChildren<TextMeshProUGUI>().text = p_Text;

            var l_Button = l_ButtonObject.GetComponent<UnityEngine.UI.Button>();
            l_Button.onClick.RemoveAllListeners();
            l_Button.onClick.AddListener(() => p_Action());

            if (p_PreferedWidth.HasValue)
                l_ButtonObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth    = p_PreferedWidth.Value;
            if (p_PreferedHeight.HasValue)
                l_ButtonObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight   = p_PreferedHeight.Value;

            if (!string.IsNullOrEmpty(p_HoverHint))
            {
                HMUI.HoverHint l_HoverHint = l_ButtonObject.GetComponent<HMUI.HoverHint>() ?? l_ButtonObject.AddComponent<HMUI.HoverHint>();
                l_HoverHint.text = p_HoverHint;
                l_HoverHint.SetField("_hoverHintController", UnityEngine.Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
            }

            l_ButtonObject.SetActive(true);
            return l_ButtonObject.GetComponent<UnityEngine.UI.Button>();
        }
    }
}

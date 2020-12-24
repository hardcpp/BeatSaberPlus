using BeatSaberMarkupLanguage.Parser;
using System.Linq;
using TMPro;

using BSMLColorSetting = BeatSaberMarkupLanguage.Components.Settings.ColorSetting;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Color setting helper
    /// </summary>
    public class ColorSetting
    {
        /// <summary>
        /// Setup a color setting
        /// </summary>
        /// <param name="p_Setting">Setting to setûp</param>
        /// <param name="p_Action">Action on change</param>
        /// <param name="p_Value">New value</param>
        /// <param name="p_RemoveLabel">Should remove label</param>
        public static void Setup(BSMLColorSetting p_Setting, BSMLAction p_Action, UnityEngine.Color p_Value, bool p_RemoveLabel)
        {
            p_Setting.gameObject.SetActive(false);

            p_Value.a = 1.0f;
            p_Setting.CurrentColor = p_Value;

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            p_Setting.updateOnChange = true;

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

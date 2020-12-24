using BeatSaberMarkupLanguage.Parser;
using System.Linq;
using TMPro;

using BSMLIncrementSetting = BeatSaberMarkupLanguage.Components.Settings.IncrementSetting;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// Increment setting helper
    /// </summary>
    public class IncrementSetting
    {
        /// <summary>
        /// Setup a increment setting
        /// </summary>
        /// <param name="p_Setting">Setting to setup</param>
        /// <param name="p_Action">Action on change</param>
        /// <param name="p_Formatter">Value formatter</param>
        /// <param name="p_Value">New value</param>
        /// <param name="p_RemoveLabel">Should remove label</param>
        public static void Setup(BSMLIncrementSetting p_Setting, BSMLAction p_Action, BSMLAction p_Formatter, float p_Value, bool p_RemoveLabel)
        {
            p_Setting.gameObject.SetActive(false);

            if (p_Formatter != null)
                p_Setting.formatter = p_Formatter;

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

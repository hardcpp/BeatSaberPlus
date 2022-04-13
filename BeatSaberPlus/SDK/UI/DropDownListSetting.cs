using BeatSaberMarkupLanguage.Parser;
using IPA.Utilities;
using TMPro;

using BSMLDropDownListSetting = BeatSaberMarkupLanguage.Components.Settings.DropDownListSetting;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// BSML DropDownListSetting helper
    /// </summary>
    public class DropDownListSetting
    {
        /// <summary>
        /// Setup a list setting
        /// </summary>
        /// <param name="p_Setting">Setting to setûp</param>
        /// <param name="p_Action">Action on change</param>
        /// <param name="p_RemoveLabel">Should remove label</param>
        public static void Setup(BSMLDropDownListSetting p_Setting, BSMLAction p_Action, bool p_RemoveLabel, float p_NewWidthPct = 1f)
        {
            p_Setting.gameObject.SetActive(false);

            if (p_Action != null)
                p_Setting.onChange = p_Action;

            p_Setting.updateOnChange = true;

            if (p_RemoveLabel)
            {
               UnityEngine.GameObject.Destroy(p_Setting.transform.parent.Find("Label").gameObject);

               UnityEngine.RectTransform l_RectTransform = p_Setting.gameObject.transform as UnityEngine.RectTransform;
               l_RectTransform.anchorMin = new UnityEngine.Vector2(1f - p_NewWidthPct, 0f);
               l_RectTransform.anchorMax = new UnityEngine.Vector2(p_NewWidthPct, 1f);
               l_RectTransform.sizeDelta = UnityEngine.Vector2.one;
               //
               //p_Setting.gameObject.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth = -1f;
            }

            p_Setting.gameObject.SetActive(true);

            /// Patch for rich text & style
            var l_Text = p_Setting.dropdown.GetField<TextMeshProUGUI, HMUI.SimpleTextDropdown>("_text");
            if (l_Text)
            {
                l_Text.rectTransform.SetInsetAndSizeFromParentEdge(UnityEngine.RectTransform.Edge.Bottom,   0, 0);
                l_Text.rectTransform.SetInsetAndSizeFromParentEdge(UnityEngine.RectTransform.Edge.Top,      0, 0);
                l_Text.rectTransform.SetInsetAndSizeFromParentEdge(UnityEngine.RectTransform.Edge.Left,     0, 0);
                l_Text.rectTransform.SetInsetAndSizeFromParentEdge(UnityEngine.RectTransform.Edge.Right,    0, 0);
                l_Text.rectTransform.anchorMin = UnityEngine.Vector2.zero;
                l_Text.rectTransform.anchorMax = UnityEngine.Vector2.one;
                l_Text.alignment    = TextAlignmentOptions.MidlineLeft;
                l_Text.overflowMode = TextOverflowModes.Ellipsis;
                l_Text.margin       = new UnityEngine.Vector4(2.5f, 0, 10, 0);
                l_Text.richText     = true;
            }
        }
    }
}

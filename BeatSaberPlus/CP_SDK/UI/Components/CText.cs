using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// CText component
    /// </summary>
    public abstract class CText : MonoBehaviour
    {
        public abstract RectTransform   RTransform  { get; }
        public abstract LayoutElement   LElement    { get; }
        public abstract TextMeshProUGUI TMProUGUI   { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get text
        /// </summary>
        /// <returns></returns>
        public string GetText()
            => TMProUGUI.text;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set align
        /// </summary>
        /// <param name="p_Align">New align</param>
        /// <returns></returns>
        public CText SetAlign(TextAlignmentOptions p_Align)
        {
            TMProUGUI.alignment = p_Align;
            return this;
        }
        /// <summary>
        /// Set alpha
        /// </summary>
        /// <param name="p_Alpha">New alpha</param>
        /// <returns></returns>
        public CText SetAlpha(float p_Alpha)
        {
            TMProUGUI.alpha = p_Alpha;
            return this;
        }
        /// <summary>
        /// Set color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public CText SetColor(Color p_Color)
        {
            TMProUGUI.color = p_Color;
            return this;
        }
        /// <summary>
        /// Set font size
        /// </summary>
        /// <param name="p_Size">New size</param>
        /// <returns></returns>
        public CText SetFontSize(float p_Size)
        {
            TMProUGUI.fontSize = p_Size * UISystem.FontScale;
            return this;
        }
        /// <summary>
        /// Set font sizes
        /// </summary>
        /// <param name="p_MinSize">New size</param>
        /// <param name="p_MaxSize">New size</param>
        /// <returns></returns>
        public CText SetFontSizes(float p_MinSize, float p_MaxSize)
        {
            TMProUGUI.fontSizeMin = p_MinSize * UISystem.FontScale;
            TMProUGUI.fontSizeMax = p_MaxSize * UISystem.FontScale;
            return this;
        }
        /// <summary>
        /// Set margins
        /// </summary>
        /// <param name="p_Left">Left margin</param>
        /// <param name="p_Top">Top margin</param>
        /// <param name="p_Right">Right margin</param>
        /// <param name="p_Bottom">Bottom margin</param>
        /// <returns></returns>
        public CText SetMargins(float p_Left, float p_Top, float p_Right, float p_Bottom)
        {
            TMProUGUI.margin = new Vector4(p_Left, p_Top, p_Right, p_Bottom);
            return this;
        }
        /// <summary>
        /// Set overflow mode
        /// </summary>
        /// <param name="p_OverflowMode">New overflow mdoe</param>
        /// <returns></returns>
        public CText SetOverflowMode(TextOverflowModes p_OverflowMode)
        {
            TMProUGUI.overflowMode = p_OverflowMode;
            return this;
        }
        /// <summary>
        /// Set style
        /// </summary>
        /// <param name="p_Style">New style</param>
        /// <returns></returns>
        public CText SetStyle(FontStyles p_Style)
        {
            TMProUGUI.fontStyle = p_Style;
            return this;
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Text">New text</param>
        /// <returns></returns>
        public CText SetText(string p_Text)
        {
            TMProUGUI.text = p_Text;
            return this;
        }
        /// <summary>
        /// Set wrapping
        /// </summary>
        /// <param name="p_Wrapping">New state</param>
        /// <returns></returns>
        public CText SetWrapping(bool p_Wrapping)
        {
            TMProUGUI.enableWordWrapping = p_Wrapping;
            return this;
        }
    }
}
